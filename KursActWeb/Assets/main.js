$(document).ready(function() {

});

function RedrawChart(selector, percent, donut) {
    // Rounding value to nearest int
    percent = Math.round(percent);
    if (percent > 100) {
        percent = 100;
    } else if (percent < 0) {
        percent = 0;
    }

    //Calculating degree of donut
    var deg = Math.round(360 * (percent / 100));

    if (percent > 50) {
        $(selector + ' .chart').css('clip', 'rect(auto, auto, auto, auto)');
        $(selector + ' .right-side').css('transform', 'rotate(180deg)');
    } else {
        $(selector + ' .chart').css('clip', 'rect(0, 1em, 1em, 0.5em)');
        $(selector + ' .right-side').css('transform', 'rotate(0deg)');
    }

    if (donut) {
        $(selector + ' .right-side').css('border-width', '0.1em');
        $(selector + ' .left-side').css('border-width', '0.1em');
        $(selector + ' .shadow').css('border-width', '0.1em');
    } else {
        $(selector + ' .right-side').css('border-width', '0.5em');
        $(selector + ' .left-side').css('border-width', '0.5em');
        $(selector + ' .shadow').css('border-width', '0.5em');
    }
    $(selector + ' .num').text(percent);
    $(selector + ' .left-side').css('transform', 'rotate(' + deg + 'deg)');
}

