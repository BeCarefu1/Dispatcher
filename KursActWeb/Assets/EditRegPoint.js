//Обновление данных в БД при измении их на странице

//Обновление инфы о Потребителе
function Update_Consumer(callerObj) {
    let formData = new FormData(document.forms.consumerForm);
    let xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4 && xhr.status === 200) {
            Input_IndicateSuccess(callerObj);
        }
    }
    xhr.open("POST", '/RegPoint/UpdateConsummer', true);
    //xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    xhr.send(formData);
}

//Обновление инфы в InstallAct
function Update_InstallAct(callerObj, isGroupBtn) {
    let formData = new FormData(document.forms.installActForm);
    let xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4 && xhr.status === 200) {
            Input_IndicateSuccess(callerObj, isGroupBtn);
        }
    }
    xhr.open("POST", '/RegPoint/UpdateInstallAct', true);
    //xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    xhr.send(formData);
}

//Обновление флагов
function Update_RegPointFlags(sender) {
    let formData = new FormData(document.forms.regPointFlagsForm);
    let xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4 && xhr.status === 200) {
            if (sender == 'ImportConsummerData') {
                $('#BtnSendToBashesk').addClass('d-sm-none');
                $('#SendToBasheskIndicator').fadeIn();
                setTimeout(function () { $('#SendToBasheskIndicator').fadeOut(); }, 5000);
            }
        }
    }
    xhr.open("POST", '/RegPoint/UpdateRegPointFlags', true);
    //xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    xhr.send(formData);
}

//Удаление точки учета
function Delete(id) {
    Swal.fire({
        title: 'Уверены что хотите удалить точку учета?',
        text: "Это действие отменить нельзя!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Да, удалить ТУ!'
    }).then((result) => {
        if (result.value) {
            fetch('/RegPoint/Delete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: "id=" + encodeURIComponent(id)
            }).then(resp => resp.text()).then(txt => alert(txt))
                .catch(() => alert('Ошибка'));
        }
    })
        
}

//Скачать акт допуска
function GetExcelAct(id) {
    fetch('/GetFile/GetExcel_Act', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: "Id=" + encodeURIComponent(id)
    }).then(resp => resp.text()).then(txt => alert(txt))
        .catch(() => alert('Ошибка'));
}


// Окрашивание инпута в зеленый цвет в случае успеха

function Input_IndicateSuccess(callerObj, isGroupBtn) {
    if (callerObj.id == "O_Address_Link") {
        $("#regPointObjectIndicator").fadeIn();
        setTimeout(function () { $("#regPointObjectIndicator").fadeOut(); }, 5000);
    } else {
        if (isGroupBtn) {
            callerObj.classList.add("save-success-btn");
        } else {
            callerObj.classList.add("save-success");
        }
        setTimeout(function() { Input_RevertToDefault(callerObj) }, 5000);
    } 
}

// Возврат обычного вида инпуту после заданного времени
function Input_RevertToDefault(callerObj) {
    callerObj.classList.remove("save-success");
    callerObj.classList.remove("save-success-btn");
}

// Update coefficient inputs on select
function Set_KoefficientInputs() {
    $(".KoefficientInput").val($("#TT_Koefficient").val());
}

// Скопировать юридический адрес в адрес юридического
function Get_U_Address(callerObj) {
    $("#O_Index").val($("#U_Index").val());
    $("#O_Local").val($("#U_Local").val());
    $("#O_Local_Secondary").val($("#U_Local_Secondary").val());
    $("#O_Street").val($("#U_Street").val());
    $("#O_House").val($("#U_House").val());
    $("#O_Build").val($("#U_Build").val());
    $("#O_Flat").val($("#U_Flat").val());
    Update_Consumer(callerObj);
}

//
// Toggles handler
//

// IsCommReg toggle handler
function SetIsComReg(command) {
    $("#IsCommReg").val(command);
    if (command) {
        $("#badgeIsCommRegTrue").show();
        $("#badgeIsCommRegFalse").hide();
    } else {
        $("#badgeIsCommRegTrue").hide();
        $("#badgeIsCommRegFalse").show();
    }
}

//IsOutFlow toggle handler
function SetIsOutFlow(command) {
    $("#IsOutFlow").val(command);
}

$(document).ready(function () {
    // Syncing IsComReg value and it's toggle
    if ($("#IsCommReg").val()) { //if it's NOT blank
        $("#IsCommRegProxyTrue").addClass("active");
        $("#IsCommRegProxyFalse").removeClass("active");
        $("#badgeIsCommRegTrue").show();
        $("#badgeIsCommRegFalse").hide();
    } else {
        $("#IsCommRegProxyTrue").removeClass("active");
        $("#IsCommRegProxyFalse").addClass("active");
        $("#badgeIsCommRegTrue").hide();
        $("#badgeIsCommRegFalse").show();
    }

    // Syncing IsOutFlow value and it's toggle
    if ($("#IsOutFlow").val()) { //if it's NOT blank
        $("#IsOutFlowProxyTrue").addClass("active");
        $("#IsOutFlowProxyFalse").removeClass("active");
    } else {
        $("#IsOutFlowProxyTrue").removeClass("active");
        $("#IsOutFlowProxyFalse").addClass("active");
    }

    // Set default option to select
    $("#TT_Koefficient").val($(".KoefficientInput").val());

    // Hide save indicators
    $("#regPointFlagsFormIndicator").hide();
    $("#regPointObjectIndicator").hide();
    $('#SendToBasheskIndicator').hide();

    // Hide SendToBashesk button if ImportConsummerData=false
    if ($('#ImportConsummerData').val() === '') {
        $('#BtnSendToBashesk').addClass('d-sm-none');
    }
});


function SendToBashEskConfirmation() {
    Swal.fire({
        title: 'Запросить данные в ЭСКБ?',
        text: "Отправится запрос на заполнение данных о потребителе",
        type: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Да, запросить!',
        cancelButtonText: 'Нееет'
    }).then((result) => {
        if (result.value) {
            $('#ImportConsummerData').val('');
            Update_RegPointFlags('ImportConsummerData');
        }
    })
}