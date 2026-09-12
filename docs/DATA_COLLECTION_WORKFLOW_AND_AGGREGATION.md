# Early Warning Data Collection, Workflow, and Aggregation Reference

Last verified against the codebase: 2026-08-12

This is the canonical maintenance document for the weekly Early Warning report. It explains data ownership, collection and validation rules, configurable/master data, table relationships, approval workflow, aggregation, snapshots, dashboards, and the final regional output. Update this document whenever a wizard field, model, workflow state, aggregation formula, or lookup table changes.

## 1. Authoritative design principles

1. A report is identified by Woreda and reporting week. `WeeklyReportSubmission` is the workflow envelope; the domain tables contain the actual answers.
2. `WeeklyReportSubmission.Status` is the authoritative approval state. Legacy `ApprovalStatus`, `ReportStatus`, `Status`, zonal approval, and regional approval fields in individual domain models must not determine whether data enters an official aggregate.
3. One Woreda can have only one submission for a reporting week. The week is normalized to Monday by `GetReportingWeekStart`.
4. Draft data is editable. Submitted data is locked unless Zone returns it to Woreda.
5. Zone and Region reviewers read existing Woreda data in review mode; they do not create replacement domain records.
6. Aggregates use only the workflow records allowed for the current hierarchy and stage.
7. Counts and quantities are summed, categorical values are counted/grouped, rates are recalculated, prices are averaged by type, and narrative text retains its Woreda source.
8. Live aggregates support review. Approval snapshots preserve the approved output for dashboard/reporting use.

## 2. Administrative hierarchy and access scope

`Locations` is a self-referencing hierarchy through `ParentId`:

```text
Country
└── Region
    └── Zone
        └── Woreda
            └── Kebele
```

`ApplicationUser.LocationId` and `UserLocationLevl` define the user's scope.

| Role | Collection/review scope | Official data visible |
|---|---|---|
| Data Encoder | Own Woreda | Own non-draft submissions; returned reports become editable |
| ZoneAdmin | Woredas whose `ParentId` is the Zone user's location | Submitted Woredas for review; Zone-approved data for aggregation |
| RegionAdmin | Woredas whose Zone belongs to the Region user's location | Zone-approved groups for review; Region-approved data for final dashboard |

Cross-scope access is rejected by controller authorization and location relationship checks.

## 3. Core workflow tables

### `WeeklyReportSubmissions`

One row represents one Woreda/week workflow envelope.

| Field | Meaning |
|---|---|
| `Id` | Submission identifier used by workflow history and animal migration routes |
| `WoredaId` | Owning Woreda |
| `StartDate`, `EndDate` | Exact reporting interval used to locate domain rows |
| `ReportingWeekStart` | Normalized Monday used to prevent duplicates |
| `Status` | Authoritative workflow status |
| `CreatedById`, `CreatedAt` | Original creator/audit data |
| `LastActionAt` | Most recent workflow change |
| `DraftDataJson` | Autosaved form fields before final domain-table persistence |
| `RowVersion` | SQL row-version used for optimistic concurrency |

Unique constraints:

- `(WoredaId, StartDate, EndDate)`
- `(WoredaId, ReportingWeekStart)`

### `WeeklyReportWorkflowHistories`

Append-only audit trail linked by `WeeklyReportSubmissionId`. It records actor, actor location/level, action, previous status, new status, comment, and time. Rejection comments are mandatory. Approval comments are optional. Do not overwrite history rows.

### `WeeklyAggregateSnapshots`

Stores approved JSON output by hierarchy and week.

| Field | Meaning |
|---|---|
| `ScopeLocationId` | Zone or Region whose output was generated |
| `ScopeLevel` | Hierarchy level of that scope |
| `StartDate`, `EndDate` | Reporting interval |
| `ApprovalStatus` | Workflow stage represented by the snapshot |
| `SourceReportCount` | Number of contributing Woreda submissions |
| `GeneratedAt`, `GeneratedById` | Audit metadata |
| `AggregateJson` | Serialized `WeeklyAggregateViewModel` |

The unique key is `(ScopeLocationId, ScopeLevel, StartDate, EndDate, ApprovalStatus)`. Saving the same scope/week/stage replaces the previous snapshot. Regional dashboard output prefers the most recent regional snapshot and falls back to a live calculation if none exists.

## 4. Workflow from collection to final regional output

```text
Woreda starts form
  → Draft autosave
  → validation and transactional upsert of all domain tables
  → SubmittedToZone (locked)
      ├─ Zone rejects → ZoneRejectedToWoreda → Woreda edits/resubmits
      └─ Zone approves → ZoneApproved → Zone snapshot refreshed
             ├─ Region rejects Zone group → RegionRejectedToZone
             │      → Zone reviews/resubmits affected Woreda reports
             └─ Region approves Zone group → RegionApproved
                    → regional snapshot refreshed
                    → final regional dashboard/output
```

### Status transitions

| Current | Actor/action | Next | Editability/inclusion |
|---|---|---|---|
| none | Woreda autosave | `Draft` | Editable; excluded |
| `Draft` | Woreda submits | `SubmittedToZone` | Locked; awaiting Zone |
| `ZoneRejectedToWoreda` | Woreda resubmits | `SubmittedToZone` | Locked; awaiting Zone |
| `SubmittedToZone` | Zone approves | `ZoneApproved` | Included in Zone/Region review aggregate |
| `SubmittedToZone` | Zone rejects with comment | `ZoneRejectedToWoreda` | Excluded; editable by Woreda |
| `RegionRejectedToZone` | Zone approves/resubmits | `ZoneApproved` | Re-enters regional queue |
| `RegionRejectedToZone` | Zone rejects to Woreda | `ZoneRejectedToWoreda` | Excluded; editable by Woreda |
| `ZoneApproved` | Region approves Zone group | `RegionApproved` | Included in final regional output |
| `ZoneApproved` | Region rejects Zone group with comment | `RegionRejectedToZone` | Returned to Zone; not final |

Every successful transition creates a history row. `RowVersion` prevents two reviewers from silently applying conflicting decisions.

## 5. Common collection and validation rules

- Woreda and valid `StartDate <= EndDate` are required.
- All submitted numbers are checked for invalid, negative, overflow, `NaN`, and infinity values. Model-specific `[Range]` attributes remain authoritative for exact limits.
- The selected Woreda must match the logged-in Data Encoder's location.
- Selected Kebeles must be active children of the Woreda.
- A Kebele count cannot exceed the configured active Kebele count for that Woreda.
- Human-water selected Kebele count must equal `NumberOfAffectedKebeles` when a problem exists.
- An epidemic marked present requires at least one epidemic disease type.
- An accident marked present requires at least one accident type.
- Inward human migration requires an origin; outward migration requires a destination.
- Inward animal migration requires an origin; outward animal migration requires a destination.
- Animal-migration counts must be `0..1,000,000,000`; an `Other` type with a positive count requires its text name.
- Distributed assistance cannot exceed provided assistance.
- Accident damage percentage must be `0..100`.
- Grain/animal weekly prices are `0..100,000`; price difference is `-100,000..100,000`; change percentage is `-100..1,000`.
- Final submission writes all domain changes and workflow changes inside a database transaction. Failure rolls the transaction back.
- Upsert identity is normally `(WoredaId, StartDate, EndDate)`. Child/type rows are replaced on resubmission so removed selections do not remain stale.
- The prior price for a type is the latest row for the same Woreda/type whose `EndDate` is before the current `StartDate`.

## 6. Configurable/master variables and relationships

These tables control available selections without changing the main weekly schema. Deactivate a value with `IsActive` when supported instead of deleting a value already referenced by historical reports.

| Configurable table/value | Main relation | Use |
|---|---|---|
| `Locations` | All main tables through `WoredaId`; Kebele junctions; migration origin/destination | Administrative scope and selectable locations |
| `CropPestAndDesease` | `CropPestImpactDetails.CropPestAndDeseaseId` | Configurable crop pest/disease types |
| `AnimalDisease` | `AnimalDiseaseImpactDetails.AnimalDiseaseId` | Configurable animal disease types |
| `EpidemicDiseaseTypes` | `EpidemicDiseaseSelections.EpidemicDiseaseTypeId` | Configurable human epidemic types |
| `AccidentTypes` | `AccidentDetails.AccidentTypeId` | Configurable accident types |
| `GrainTypes` | `GrainPricePerQuintals.GrainTypeId` | Configurable grain price rows |
| `AnimalTypes` | `AnimalPricePerUnits.AnimalTypeId` | Configurable animal price rows |
| `SupplyTypes` | `WeeklyProvidedDetails.SupplyTypeId` | Configurable assistance types |
| `GrainIncreaseDecreases` | Grain-supply detail/reason records | Configurable grain supply reasons |
| `GrainPriceIncreaseDecreases` | Grain-price detail/reason records | Configurable grain price reasons |
| `AnimalIncreaseDecreases` | Animal-supply detail/reason records | Configurable animal supply reasons |
| `AnimalPriceIncreaseDecreases` | Animal-price detail/reason records | Configurable animal price reasons |
| `SupplyStatus` enum | Grain/animal supply and price parent tables | Status category counted by Woreda |
| `WeeklyStatus` enum | Grain/animal price rows | Increased/decreased/same classification |
| `Measurement` enum | `WeeklyProvidedDetails` | Prevents mixing quantities with different units conceptually |
| `AnimalMigrationDirection`, `MigratingAnimalType` enums | `AnimalMigrationRecords` | Route grouping dimensions |
| `WeeklySubmissionStatus` enum | `WeeklyReportSubmissions` and snapshots | Authoritative lifecycle |
| ASP.NET Identity roles | Controller policies | Actor permissions |

When adding or changing a configurable variable:

1. Preserve existing primary keys used by historical data.
2. Update its administration UI and active filtering.
3. Update wizard lookup population and hydration.
4. Update child validation and aggregation grouping labels.
5. Add a migration if schema or enum persistence changes.
6. Update this document and test historical report rendering.

## 7. Domain tables, collection rules, and aggregation rules

All rules below apply only after the parent `WeeklyReportSubmission` passes the hierarchy's inclusion filter.

| Wizard area / main table | Child or configurable relationship | Collection/update rule | Aggregate/output rule |
|---|---|---|---|
| Rainfall — `RainfallReports` | Kebele selections serialized in JSON; Woreda Kebeles from `Locations` | Counts by coverage and amount; selected Kebeles must belong to Woreda | `SUM` Kebele counts for full, partial, no rain, no data, low, medium, high. Preserve narrative/Kebele source data for detail review |
| Farming — `FarmingActivities` | `CropGrowthType` enum | Meher and Belg/Autumn plan, ploughed, sown, harvested, residual-moisture areas | `SUM` hectares for each activity; do not sum growth enum—use distribution if exposed later |
| Crop pest/disease — `CropPestAndDeseaseReports` | `CropPestImpactDetails` → `CropPestAndDesease` | Presence flag; demographics; affected hectares/crop; one detail per selected type. Existing children are deleted/rebuilt | `SUM` affected hectares and all demographic categories. Future per-type output groups by pest/disease ID. Remarks remain Woreda-attributed |
| Pasture — `PastureStatuses` | Boolean `Enough` | Status, affected animals, demographics, remarks | `SUM` animals and demographics; `COUNT` Woredas where `Enough=false`; preserve remarks |
| Animal water — `AnimalWaterSupplyStatuses` | Woreda/Kebele configuration | Enough flag, shortage/water Kebele counts, demographics | `SUM` Kebele counts and demographics; `COUNT` Woredas where insufficient |
| Animal health — `AnimalHealthStatuses` | `AnimalDiseaseImpactDetails` → `AnimalDisease` | Status and demographics; one child per selected disease; children rebuilt on update | `SUM` demographics; `COUNT` problem Woredas. Future per-disease output groups by disease ID and sums affected animals/people. Remarks remain attributed |
| Human epidemic — `EpidemicDiseases` | `EpidemicDiseaseSelections` → `EpidemicDiseaseTypes`; optional Kebele junction | If present, select at least one type; affected Kebele count limited by Woreda; detailed demographics include pregnant/lactating | `SUM` affected Kebeles, all demographics, pregnant women, and lactating mothers. Future per-type output groups by epidemic type |
| Human drinking water — `HumanDrinkWaterIssues` | `HumanDrinkWaterKebeles` → `Locations` | Presence flag, affected-Kebele count, exact selected Kebele match, demographics | `SUM` affected Kebeles and demographics. Keep selected Kebele identities for drill-down/deduplication |
| Grain supply — `GrainSupplys` | Optional `GrainSupplyDetails` → reason master | Status plus notes | `COUNT` Woredas grouped by `SupplyStatus`; never sum an enum. Notes remain source-attributed when added to narrative output |
| Grain price — `GrainPrices`; `GrainPricePerQuintals` | Rows → `GrainTypes`; optional reason details | One current price row per configured grain type; previous price derived; rows replaced on resubmission | `AVERAGE` current price grouped by grain type because no quantity weight exists. If market-volume data is added, change to weighted average. Never sum prices |
| Animal supply — `AnimalSupplys` | Optional `AnimalSupplyDetails` → reason master | Status plus notes | `COUNT` Woredas grouped by `SupplyStatus`; preserve notes by Woreda |
| Animal price — `AnimalPrice`; `AnimalPricePerUnits` | Rows → `AnimalTypes`; optional reason details | One price per configured animal type; previous price derived; rows replaced on resubmission | `AVERAGE` price grouped by animal type; use weighted average only after quantity/volume is collected |
| Weekly accidents — `WeeklyAccidents` | `AccidentDetails` → `AccidentTypes` | If present, select a type; detail contains land, damage rate and demographics; children rebuilt | `SUM` damaged hectares, affected households and affected people. Do not average damage rates across unlike land areas; a future regional damage rate must be weighted/recalculated from a defined denominator |
| Human migration — `Migrations` | `MigrationDetails` → origin/destination `Locations` | Presence, direction, route/reason, demographics; children rebuilt | `SUM` demographics. Recommended detailed output groups by inward/outward, origin, destination, and reason while retaining routes |
| Death — `Deaths` | `DeathDetails` | Presence, reason, demographics; children rebuilt | `SUM` all death demographic categories; retain reason as grouped/source text |
| Assistance recipients — `AssistanceRecipients` | None beyond Woreda | Demographic beneficiary counts | `SUM` every demographic category; grand total is the sum of all categories |
| Assistance provided — `WeeklyProvideds` | `WeeklyProvidedDetails` → `SupplyTypes`, `Measurement` | Per type/unit: provided and distributed quantities; distributed <= provided; children rebuilt | Group by supply type **and measurement unit**, then `SUM` quantities. Overall distribution percent = total distributed / total provided × 100; never average row percentages |
| Other problem — `OtherProblems` | Free-text `ProblemName` | If a problem exists, name is required; collect demographics and notes | `COUNT` affected Woredas; `SUM` demographics. Normalize identical names only for optional grouping; otherwise retain each Woreda's text |
| Animal migration — `AnimalMigrationRecords` | Parent workflow; origin/destination `Locations`; direction/type enums | Only positive rows saved; origin/destination required by direction; `Other` requires name. Existing submission routes are deleted/rebuilt | Group by direction + origin + destination + animal type + other type, then `SUM NumberOfAnimals` |
| School situation — `SchoolReports` | Woreda/week; five solution-category flags | If a teaching/learning problem exists, cause, three major solutions, satisfaction decision and at least one solution category are required. Dropout counts and closed schools are nonnegative; dropout age is required when dropouts exist; `Other` requires description | `COUNT` problem Woredas and solution categories; `SUM` male/female/total dropouts and closed schools; calculate dropout-weighted average age. Causes, three solutions and other descriptions remain Woreda-attributed |
| Review comments — `WeeklyReportWorkflowHistories` | Workflow and user/location | Rejection requires comment; every decision appended | Never concatenate into an anonymous summary. List source Woreda/location, action, author/time, and comment |

### Demographic total convention

Unless a report explicitly defines another denominator, “affected people” is the sum of:

```text
male household heads + female household heads
+ male family members + female family members
+ male children + female children
+ male youth + female youth
+ male elderly + female elderly
+ male disabled + female disabled
```

Pregnant and lactating fields are reported separately and are not automatically added to this grand total because they may overlap other female categories.

## 8. Aggregation inclusion by hierarchy

### Weekly schedule and duplicate rule

- Each Woreda reporting period is Monday through Wednesday of the same week.
- Existence and uniqueness are checked for the particular `WoredaId` and its Monday-normalized reporting week; the canonical displayed period is Monday–Wednesday.
- Selecting a date in the wizard normalizes the period to Monday–Wednesday. Another Woreda's report must never block the selected Woreda.
- A matching `Draft` or `ZoneRejectedToWoreda` record is reopened for editing. A submitted or approved record is available according to role permissions but cannot be duplicated.
- Zone approval or rejection is allowed through Thursday inclusive (`ReportingWeekStart + 3 days`). After Thursday the report remains readable, but the Zone decision endpoint rejects changes.

### Woreda

The Woreda stores one report/week. It does not combine other Woredas. The dashboard shows its own latest non-draft submission.

### Zone live review and output

- Approval queue: `SubmittedToZone` and `RegionRejectedToZone`.
- Official/live aggregate: `ZoneApproved`, `RegionApproved`, and `RegionRejectedToZone` records in the Zone scope.
- `Draft`, `SubmittedToZone`, and `ZoneRejectedToWoreda` do not contribute to the Zone official aggregate.
- Each Zone approval refreshes the Zone/week snapshot using the currently eligible Woredas.

### Region review and final output

- Regional queue groups `ZoneApproved` Woreda submissions by Zone + start date + end date.
- Region reviews the group and its live aggregate.
- Approval changes every `ZoneApproved` Woreda in that Zone/week group to `RegionApproved`.
- After approval, the regional snapshot is rebuilt from all `RegionApproved` Woredas in the Region for the same interval.
- The regional dashboard shows the latest Region-approved week and prefers its snapshot.

### Coverage indicator

Every official output should display `SourceReportCount`/Woreda count. A future enhancement should also display expected active Woredas and coverage such as `8 of 10 approved`; do not interpret incomplete coverage as a complete regional total.

## 9. Dashboard and regional output contract

The Manual Data Collection Form (`Weekly report format UPDATE 27.docx`) is the naming authority for every heading and field label that exists in that form. Display text must preserve its Amharic wording (with spacing normalized for screen readability). A field introduced only in the application may retain its application label because no manual equivalent exists. Whenever the manual changes, compare and update the wizard, summarized submission review, aggregation service labels, dashboards, and this document together.

The shared `_AggregateSections` view uses the same read-only review presentation as the submission's Summarized Review: a heading card for each domain and a responsive two-column grid of labeled value blocks. It renders the metric value, unit, and aggregation method (`SUM`, `COUNT`, `AVERAGE`, `WEIGHTED AVERAGE`, or `RECALCULATED`). The method remains visible for auditability, but is supporting metadata rather than a generic table column.

`/WeeklyWorkflow` is the aggregate-output page and does not display the Woreda report table. Individual Woreda reports are exposed through the separate **የሪፖርት ግምገማ** link (`/WeeklyWorkflow?mode=details`), while actionable Zone decisions remain in the approval queue (`mode=approval`). This separation keeps aggregation presentation distinct from record-level review and workflow actions.

Text aggregation follows these rules:

- preserve the source field and contributing location for traceability;
- group entries by their manual/source field label;
- join all contributing location statements into one coherent paragraph using Amharic punctuation;
- do not render each Woreda statement as a separate list item;
- do not mathematically combine free text or discard duplicates silently;
- workflow approval/rejection comments remain a separate labeled narrative because they are not fields in the manual form.

Animal-migration routes remain tabular because origin, destination, direction, animal type, and count are structured dimensions, not free text. Their column labels use the nearest wording from the manual migration section where an equivalent exists.

The final regional output contains:

- reporting interval and Region scope;
- contributing Woreda/report count;
- numeric sections for every implemented wizard domain;
- price averages by configured type;
- supply-status distributions;
- animal migration route totals;
- Woreda-attributed narrative and workflow comments;
- snapshot generation time and actor in storage.

Exports introduced later (Excel, PDF, API) must consume the same `WeeklyAggregateViewModel` or snapshot JSON rather than reimplementing formulas independently.

## 10. Performance and consistency rules

- Filter by eligible submission IDs/Woreda/date before aggregation.
- Use `AsNoTracking` for read-only aggregation queries.
- Prefer database-side `SUM`, `COUNT`, grouping, and projection as volume grows. The current service still materializes several scoped lists before final in-memory filtering; preserve scope filters and progressively move remaining calculations into SQL.
- Do not query all historical rows and filter only in the UI.
- Use snapshots for final approved dashboards and exports.
- Recalculate only the affected scope/week after approval/resubmission.
- Never update a final snapshot by editing its JSON manually; regenerate it from authoritative domain records.
- Retain row-version checks on workflow decisions.
- A non-editable submission status is valid only when the workflow history contains a Woreda `Submit` or `Resubmit` event. A locked status without either event is an orphaned/failed workflow row and is automatically restored to `Draft` without deleting its saved domain data.
- New `WeeklyReportSubmission` instances must explicitly default to `Draft`; never rely on the enum/database zero value because the legacy zero value is `SubmittedToZone`.
- Use a transaction when domain rows and workflow state change together.

Recommended indexes:

- `WeeklyReportSubmissions(WoredaId, ReportingWeekStart)` unique
- `WeeklyReportSubmissions(WoredaId, StartDate, EndDate)` unique
- Domain parent tables: `(WoredaId, StartDate, EndDate)`
- Child tables: parent foreign key plus configurable type ID
- `WeeklyAggregateSnapshots(ScopeLocationId, ScopeLevel, StartDate, EndDate, ApprovalStatus)` unique

## 11. Known legacy and maintenance cautions

- Many domain models still contain historical approval fields. They are not the weekly workflow authority. New code must use `WeeklyReportSubmission.Status`.
- Some parent/detail/reason tables exist from older CRUD flows but are not all included in the current wizard's visible aggregation. Confirm the wizard actually persists a field before exposing it as an official metric.
- `CropGrowths` exists in the older combined report flow but is not currently a distinct section of `RegistrationWithardViewModel`; farming carries a `CropGrowth` enum. Do not assume both flows are identical.
- Current grain and animal price aggregation is a simple average because quantity/market volume is not collected. Document and migrate the formula if weighting variables are introduced.
- Assistance quantities must not be combined across unlike `Measurement` units in future grouped output.
- Several decimal properties have EF precision warnings. Before changing financial/quantity precision, explicitly configure SQL precision and create a reviewed migration to avoid truncation.
- An empty migration named `20260812164910_AddWeeklyAggregateSnapshots` precedes the actual storage migration `20260812165500_CreateWeeklyAggregateSnapshotStorage`; retain migration history order unless deliberately consolidating migrations in a new database baseline.

## 12. Change checklist

For every future collection or logic update:

1. Identify the main table, child table, lookup/configuration table, and workflow envelope relationship.
2. Define validation, units, null/zero semantics, and whether the field is conditional.
3. Update wizard display, model binding, lookup population, existing-data hydration, and transactional upsert.
4. Define aggregation explicitly as `SUM`, `COUNT`, grouped distribution, weighted/simple `AVERAGE`, `RECALCULATED`, or source-attributed text.
5. Define which workflow statuses include the value at Woreda, Zone, and Region levels.
6. Update snapshot serialization compatibility and dashboard rendering.
7. Add/verify indexes and migration safety.
8. Test draft → submit → Zone reject/resubmit → Zone approve → Region reject/resubmit → Region approve.
9. Test concurrency with two reviewers and confirm rejection comments/history.
10. Test an incomplete Zone and verify coverage is visible.
11. Update this document in the same change.

## 13. Primary implementation locations

| Responsibility | Source |
|---|---|
| Wizard collection, validation, hydration, upsert, submit | `Controllers/RegistrationWizardController.cs` |
| Workflow queues, authorization, approval/rejection | `Controllers/WeeklyWorkflowController.cs` |
| Aggregate formulas and snapshots | `Services/WeeklyAggregationService.cs` |
| Workflow/snapshot entities | `Models/WeeklyReportSubmission.cs`, `Models/WeeklyAggregateSnapshot.cs` |
| School collection entity | `Models/SchoolReport.cs` |
| Aggregate result contract | `ViewModels/WeeklyWorkflowViewModels.cs` |
| EF relationships/indexes | `Data/EarlyWarningDbContext.cs` |
| Zone/Region presentation | `Views/WeeklyWorkflow/*` |
| Shared aggregate/review-card and coherent-narrative presentation | `Views/Shared/_AggregateSections.cshtml` |
| Role-scoped dashboard | `Controllers/HomeController.cs`, `Views/Home/Index.cshtml` |
| Status and dimension enums | `Enums/Enums.cs` and domain model enum declarations |

If this document and code disagree, treat the code as current runtime behavior, correct the discrepancy, and update both together.
