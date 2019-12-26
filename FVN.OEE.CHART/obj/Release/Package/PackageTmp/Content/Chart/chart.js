$(document).ready(function () {
    //for (let i = 1; i <= 6; i++) {
    //    renderHtml('container' + i, 'numberDigital' + i, 'L'+i);
    //    renderChart('container' +i, '#numberDigital'+i, 35 + (i*10));
    //}
    $('#start input').val(dateNow);
    $('#start input').datepicker({
        clearBtn: true,
        language: "vi",
        daysOfWeekHighlighted: "0",
        //todayHighlight: true
    });
});
