// dateRangeValidator.js
(function ($) {
    $.fn.validateDateRange = function (options) {
        var settings = $.extend({
            startDateSelector: '#StartDate',
            endDateSelector: '#EndDate',
            maxDays: 8,
            errorMessage: 'The date range must be less than {maxDays} days.'
        }, options);

        var $start = $(settings.startDateSelector);
        var $end = $(settings.endDateSelector);

        function validate() {
            var startVal = $start.val();
            var endVal = $end.val();
            var errorSpan = $('#' + settings.errorSpanId);

            if (startVal && endVal) {
                var start = new Date(startVal);
                var end = new Date(endVal);
                var diffDays = Math.ceil((end - start) / (1000 * 60 * 60 * 24));

                if (diffDays >= settings.maxDays) {
                    if (!errorSpan.length) {
                        $('<span id="' + settings.errorSpanId + '" class="text-danger"></span>').insertAfter($end);
                        errorSpan = $('#' + settings.errorSpanId);
                    }
                    var msg = settings.errorMessage.replace('{maxDays}', settings.maxDays);
                    errorSpan.text(msg);
                    return false;
                } else {
                    if (errorSpan.length) errorSpan.remove();
                    return true;
                }
            }
            if (errorSpan.length) errorSpan.remove();
            return true;
        }

        $start.add($end).on('change input', validate);
        this.on('submit', function (e) {
            if (!validate()) {
                e.preventDefault();
                $end.focus();
            }
        });
        return this;
    };
})(jQuery);