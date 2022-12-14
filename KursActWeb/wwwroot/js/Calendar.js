
let positionScroll = 0;

let yearNow = (new Date()).getFullYear();
let monthNow = (new Date()).getMonth() + 1;
let nowMonth = (new Date()).getMonth() + 1;

let orderList = [];
let arr = [];
let countOrder;

let gMonth;
let gYear;

async function GetMonthold(m) {
    if (m == 'back') {
        monthNow--;
        if (monthNow == 0) {
            monthNow = 12;
            yearNow--;
        }
    }
    if (m == 'next') {
        monthNow++;
        if (monthNow == 13) {
            monthNow = 1;
            yearNow++;
        }
    }
    //Calendar(calendar, yearNow, monthNow);
    await GetCalendar(calendar, yearNow, monthNow);
}



function GetEvent(day) {
    let id = day + " " + gMonth + " " + gYear;
    $('#eventModal').modal('toggle');
    fetch('/Calendar/GetEvent/' + id).then(r => r.json()).then(data => {
        let html = data;
        document.getElementById("idModel").innerHTML = html;
        //document.getElementById("calendar").innerText = html;
        //elem.innerText(html);
    });
}


function GetPhonegram(day) {
    let id = day + " " + gMonth + " " + gYear;
    $('#eventModal').modal('toggle');
    fetch('/Calendar/GetPhonegram/' + id).then(r => r.json()).then(data => {
        let html = data;
        document.getElementById("idModel").innerHTML = html;
        //document.getElementById("calendar").innerText = html;
        //elem.innerText(html);
    });
}


async function GetCalendar(elem, year, month) {
    var month_Arr = ['ЯНВАРЬ', 'ФЕВРАЛЬ', 'МАРТ', 'АПРЕЛЬ', 'МАЙ', 'ИЮНЬ', 'ИЮЛЬ', 'АВГУСТ', 'СЕНТЯБРЬ', 'ОКТЯБРЬ', 'НОЯБРЬ', 'ДЕКАБРЬ'];
    document.getElementById('id_month').innerHTML = month_Arr[month - 1] + ' ' + year;
    let mon = month - 1; // месяцы в JS идут от 0 до 11, а не от 1 до 12
    let d = new Date(year, mon);

    gMonth = month;
    gYear = year;

    let id = getDay(d) + " " + month + " " + year;
    await fetch('/Calendar/GetCalendar/' + id).then(r => r.json()).then(data => {
        let html = data;
        document.getElementById("calendar").innerHTML = html;
    });
}



function getDay(date) { // получить номер дня недели, от 0 (пн) до 6 (вс)
    let day = date.getDay();
    if (day == 0) day = 7; // сделать воскресенье (0) последним днем
    return day - 1;
}


