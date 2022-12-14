var app = new Vue({
    el: '#app',
    data: {
        isLoading: true,
        selectedAll: false,
        substationId: 0,
        points: [],
        selectedPoints: [],
        //Search
        search: '',
        sortKey: 'RegPointId',
        reverse: false,
        findedCount: 0
    },
    computed: {
        filterOrderPoints: function () {
            let rev = this.reverse ? 'desc' : 'asc';
            var self = this;
            let excludeSearch = false;
            let searchString = this.search;
            if (this.search.charCodeAt(0) === '!'.charCodeAt(0)) {
                excludeSearch = true;
                searchString = searchString.substring(1, searchString.length);
            }
            //Filter
            let result = self.points.filter(function (point) {
                let inString =
                    point.RegPointId +
                    point.OAddress +
                    point.InstallPlace +
                    point.DeviceDescription +
                    point.Serial +
                    point.PhoneNumber;
                let result = (new RegExp(searchString, 'i').test(inString));
                if (excludeSearch) {
                    return !result;
                }
                else {
                    return result;
                }
            });
            this.findedCount = result.length;
            //Sort
            result = _.orderBy(result, this.sortKey, rev);
            return result;
        }
    },
    methods: {
        sortBy: function (sortKey) {
            this.reverse = (this.sortKey == sortKey) ? !this.reverse : false;
            this.sortKey = sortKey;
        },
        //Определяет, выделена ли строка таблицы
        isSelected: function (id) {
            return this.selectedPoints.indexOf(id) != -1;
        },
        mouseMove: function (RegPointId) {
            if (isMouseDown && isShiftDown) {
                if (!this.isSelected(RegPointId)) {
                    this.selectedPoints.push(RegPointId);
                }
            }
        },
        //Перенос выбранных точек учета в другую подстанцию
        movePointsTo: async function (points) {
            let subs = fetch('/Substation/SubstationsNear_json/' + this.substationId)
                .then(resp => resp.json())
                .then(data => data)
            const { value: subId } = await Swal.fire({
                title: 'Выбери подстанцию в которую нужно перенести точки',
                input: 'select',
                inputOptions: subs,
                inputPlaceholder: 'Выбери подстанцию',
                showCancelButton: true
            });
            if (subId) {
                fetch('/RegPoint/ReplaceTo', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Id: subId, Points: points })
                })
                .then(r => {
                    if (r.ok) {
                        ok('Точки усешно перенесены');
                    }
                });
            }
        },
        //Флаги
        switch_LinkOk: function (point) {
            fetch('/RegPoint/Update_IsLinkOk', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: "id=" + encodeURIComponent(point.RegPointId) + "&newstatus=" + encodeURIComponent(!point.IsLinkOk)
            }).then((response) => {
                if (response.ok) {
                    ok('Флаг "Связь проверена" изменен, точка учета ID: ' + point.RegPointId);
                    point.IsLinkOk = !point.IsLinkOk;
                } else { err('Ошибка сервера' + response.statusText); }
            });
            
        },
        switch_AscueChecked: function (point) {
            fetch('/RegPoint/Update_IsAscueChecked', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: "id=" + encodeURIComponent(point.RegPointId) + "&newstatus=" + encodeURIComponent(!point.IsAscueChecked)
            }).then((response) => {
                if (response.ok) {
                    ok('Флаг "Проверено для ПО АСКУЭ" изменен, точка учета ID: ' + point.RegPointId);
                    point.IsAscueChecked = !point.IsAscueChecked;
                } else { err('Ошибка сервера' + response.statusText); }
            });
        },
        switch_AscueOk: function (point) {
            fetch('/RegPoint/Update_IsAscueOk', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: "id=" + encodeURIComponent(point.RegPointId) + "&newstatus=" + encodeURIComponent(!point.IsAscueOk)
            }).then((response) => {
                if (response.ok) {
                    ok('Флаг "Работает в ПО АСКУЭ" изменен, точка учета ID: ' + point.RegPointId);
                    point.IsAscueOk = !point.IsAscueOk;
                } else { err('Ошибка сервера' + response.statusText); }
            });
        },
        switch_ReplaceOk: function (point) {
            fetch('/RegPoint/Update_IsReplace', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: "id=" + encodeURIComponent(point.RegPointId) + "&newstatus=" + encodeURIComponent(!point.IsReplace)
            }).then((response) => {
                if (response.ok) {
                    ok('Флаг "Замена ПУ" изменен, точка учета ID: ' + point.RegPointId);
                    point.IsReplace = !point.IsReplace;
                } else { err('Ошибка сервера' + response.statusText); }
            });
        }
    },
    watch: {
        selectedAll: function (val) {
            this.selectedPoints = [];
            if (val) {
                this.filterOrderPoints.forEach(item => this.selectedPoints.push(item.RegPointId));
            }
        }
    }
});

function GetPointsInfo(substationId) {
    app.isLoading = true;
    fetch('/Substation/AllPoints_json/' + substationId)
        .then(resp => resp.json())
        .then(data => app.points = data)
        .then(_ => app.isLoading = false);
}

//Обработка выделания строчек таблицы
var isMouseDown = false;
var isShiftDown = false;
document.body.onmousedown = function () {
    isMouseDown = true;
}
document.body.onmouseup = function () {
    isMouseDown = false;
}
document.body.onkeydown = function (e) {
    if (e.keyCode === 16)
        isShiftDown = true;
}
document.body.onkeyup = function (e) {
    if (e.keyCode === 16)
        isShiftDown = false;
}
document.body.onselectstart = function () {
    if (isShiftDown) return false;
}