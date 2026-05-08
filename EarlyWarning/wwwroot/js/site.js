
    $(document).ready(function () {
        $('.SectrorsDepartmentId').attr('disabled', true);
    getworedadepartments();

});

    function getworedadepartments() {
        $('.SectrorsDepartmentId').empty();
    $.ajax({
        url: '/Cr_JudicalAppealOpening/Departmentslists',
    success: function (response) {
            if (response != null && response != undefined && response.length > 0) {
        $('.SectrorsDepartmentId').attr('disabled', false);
    $('.SectrorsDepartmentId').append('<option value="" > ------- የስራ ክፍል ይምረጡ  --------</option>');
    $.each(response, function (i, data) {
        $('.SectrorsDepartmentId').append('<option value=' + data.sectrorsDepartmentId + '>' + data.departmentName + '</option>');
                });
            }
    else {
        $('.SectrorsDepartmentId').attr('disabled', true);
    $('.SectrorsDepartmentId').append('<option value="" > ------- የስራ ክፍል ዝርዝር አልተገኘም --------</option>');

            }

        }
    });
}

//To get Crim Categories with dropdown
$(document).ready(function () {
    $('.Cr_CrimeCategoryId').attr('disabled', true);
    getCrimeCategorys();

});

function getCrimeCategorys() {
    $('.Cr_CrimeCategoryId').empty();
    $.ajax({
        url: '/Cr_JudicalAppealOpening/CrimeCategoryList',
        success: function (response) {
            if (response != null && response != undefined && response.length > 0) {
                $('.Cr_CrimeCategoryId').attr('disabled', false);
                $('.Cr_CrimeCategoryId').append('<option value="" > ------- የወንጀል ምድብ ይምረጡ  --------</option>');
                $.each(response, function (i, data) {
                    $('.Cr_CrimeCategoryId').append('<option value=' + data.cr_CrimeCategoryId + '>' + data.crimeCategoryName + '</option>');
                });
            }
            else {
                $('.Cr_CrimeCategoryId').attr('disabled', true);
                $('.Cr_CrimeCategoryId').append('<option value="" > ------- የወንጀል ምድብ ይምረጡ --------</option>');

            }

        }
    });
}


//To get Crim Type with dropdown
$(document).ready(function () {
    $('.Cr_Crime_TypeId').attr('disabled', true);
    getCrimeType();

});

function getCrimeType() {
    $('.Cr_Crime_TypeId').empty();
    $.ajax({
        url: '/Cr_JudicalAppealOpening/CrimeList',
        success: function (response) {
            if (response != null && response != undefined && response.length > 0) {
                $('.Cr_Crime_TypeId').attr('disabled', false);
                $('.Cr_Crime_TypeId').append('<option value="" > ------- የወንጀል አይነት ይምረጡ  --------</option>');
                $.each(response, function (i, data) {
                    $('.Cr_Crime_TypeId').append('<option value=' + data.cr_Crime_TypeId + '>' + data.crimeTypeName + '</option>');
                });
            }
            else {
                $('.Cr_Crime_TypeId').attr('disabled', true);
                $('.Cr_Crime_TypeId').append('<option value="" > ------- የወንጀል አይነት ይምረጡ --------</option>');

            }

        }
    });
}

//To get Victoms Age List with dropdown
$(document).ready(function () {
    $('.Cr_VictimsAgeCategoryId').attr('disabled', true);
    getVageType();

});

function getVageType() {
    $('.Cr_VictimsAgeCategoryId').empty();
    $.ajax({
        url: '/Cr_VictimsAgeCategory/VictmsAgeCategory',
        success: function (response) {
            if (response != null && response != undefined && response.length > 0) {
                $('.Cr_VictimsAgeCategoryId').attr('disabled', false);
                $('.Cr_VictimsAgeCategoryId').append('<option value="" > ------- የተበዳይ እድሜ ይምረጡ  --------</option>');
                $.each(response, function (i, data) {
                    $('.Cr_VictimsAgeCategoryId').append('<option value=' + data.cr_VictimsAgeCategoryId + '>' + data.categoryList + '</option>');
                });
            }
            else {
                $('.Cr_VictimsAgeCategoryId').attr('disabled', true);
                $('.Cr_VictimsAgeCategoryId').append('<option value="" > ------- የተበዳይ እድሜ የለም --------</option>');

            }

        }
    });
}

//To get Cr_DefendantAgeCatagory List with dropdown
$(document).ready(function () {
    $('.Cr_DefendantAgeCatagoryId').attr('disabled', true);
    getDefendentageType();

});

function getDefendentageType() {
    $('.Cr_DefendantAgeCatagoryId').empty();
    $.ajax({
        url: '/Cr_VictimsAgeCategory/DefendentAgeCategory',
        success: function (response) {
            if (response != null && response != undefined && response.length > 0) {
                $('.Cr_DefendantAgeCatagoryId').attr('disabled', false);
                $('.Cr_DefendantAgeCatagoryId').append('<option value="" > ------- የተከሳሽ እድሜ ይምረጡ  --------</option>');
                $.each(response, function (i, data) {
                    $('.Cr_DefendantAgeCatagoryId').append('<option value=' + data.cr_DefendantAgeCatagoryId + '>' + data.categoryList + '</option>');
                });
            }
            else {
                $('.Cr_DefendantAgeCatagoryId').attr('disabled', true);
                $('.Cr_DefendantAgeCatagoryId').append('<option value="" > ------- የተከሳሽ እድሜ የለም--------</option>');

            }

        }
    });
}