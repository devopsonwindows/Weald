var reposTable;
var dataTable;
var diskUsageChart;
var revisionsChart;

$(document).ready(function () {
    $.ajaxSetup({ cache: false });

    reposTable = $("#reposTable");

    reposTable.on('init.dt', function () {
        getBigRepoDetails();
        drawCharts();
    });

    reposTable.on('xhr.dt', function () {
        getBigRepoDetails();

        if (diskUsageChart != null) {
            diskUsageChart.destroy();
        }
        if (revisionsChart != null) {
            revisionsChart.destroy();
        }
        drawCharts();
    });

    dataTable = reposTable.dataTable({
        'deferRender': true,
        'paging': false,
        'stateSave': true,
        'ajax': {
            'url': 'api/Repos',
            'dataSrc': ''
        },
        'language': {
            'emptyTable': 'No repositories found on the current server',
            'info': 'Showing _START_ to _END_ of _TOTAL_ repositories',
            'infoEmpty': 'Showing 0 to 0 of 0 repositories',
            'search': 'Filter:',
            'zeroRecords': 'No repositories found on the current server'
        },
        'columnDefs': [
            {
                'targets': 0,
                'render': function (data, type, row) {
                    return "<a href=\"" + row.Url + "\" target=\"_blank\">" + data + "</a>";
                },
            },
        ],
        'columns': [
            { 'data': 'Name' },
            {
                'data': 'SizeInBytes',
                'type': 'file-size',
                'render': function(data) {
                    return formatByteSize(data);
                }
            },
            { 'data': 'LatestRevision' },
            { 'data': 'LatestChangeUsername' },
            {
                'data': 'LatestChangeTimestamp',
                'type': 'string',
                'render': function (data) {
                    var dateTime = moment(data);
                    return dateTime.fromNow() + "<br /><small>(" + dateTime.format('YYYY-MM-DD hh:mm:ss A') + ")</small>";
                }
            }
        ]
    });

    setInterval(function() {
        dataTable.fnReloadAjax();
    }, 3000);
});

function formatByteSize(data) {
    if (data == null || data == 0) return '0';
    var k = 1024;
    var sizes = ['bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    var i = Math.floor(Math.log(data) / Math.log(k));
    return (data / Math.pow(k, i)).toPrecision(3) + ' ' + sizes[i];
}
// http://www.mulinblog.com/a-color-palette-optimized-for-data-visualization/
var colors = ['#F15854', '#DECF3F', '#B276B2', '#B2912F', '#F17CB0', '#60BD68', '#FAA43A', '#5DA5DA', '#4D4D4D', '#C0C0C0'];

function drawCharts() {
    var topEntries = 10;
    var data = dataTable.api().data();

    if (data == null) {
        return;
    }

    var dataArray = data.toArray();

    dataArray.sort(function (a, b) { return (a.SizeInBytes < b.SizeInBytes) ? 1 : ((b.SizeInBytes < a.SizeInBytes) ? -1 : 0); });

    var diskUsageSlice = dataArray;

    if (data.length > topEntries) {
        diskUsageSlice = dataArray.slice(0, topEntries);
    }

    var diskUsageChartData = [];

    for (var i = diskUsageSlice.length - 1; i >= 0; i--) {
        diskUsageChartData.push({
            'value': diskUsageSlice[i].SizeInBytes,
            'label': diskUsageSlice[i].Name,
            'color': colors[i]
        });
    }

    dataArray.sort(function (a, b) { return (a.LatestRevision < b.LatestRevision) ? 1 : ((b.LatestRevision < a.LatestRevision) ? -1 : 0); });

    var revisionsSlice = dataArray;

    if (data.length > topEntries) {
        revisionsSlice = dataArray.slice(0, topEntries);
    }

    var revisionsChartData = [];

    for (var j = revisionsSlice.length - 1; j >= 0; j--) {
        revisionsChartData.push({
            'value': revisionsSlice[j].LatestRevision,
            'label': revisionsSlice[j].Name,
            'color': colors[j]
        });
    }

    var ctx = $("#diskUsageChart").get(0).getContext("2d");
    diskUsageChart = new Chart(ctx).Pie(diskUsageChartData, {
        'animation': false,
        'tooltipTemplate': "<%if (label){%><%=label%> <%}%>",
    });

    ctx = $("#revisionChart").get(0).getContext("2d");
    revisionsChart = new Chart(ctx).Doughnut(revisionsChartData, {
        'animation': false,
        'tooltipTemplate': "<%if (label){%><%=label%> <%}%>",
    });
}

function getBigRepoDetails() {
    var data = dataTable.api().data();

    if (data == null) {
        return;
    }

    var largestRevisionRepo;
    var largestSizeRepo;

    var highestRevision = Number.NEGATIVE_INFINITY;
    var highestSize = Number.NEGATIVE_INFINITY;
    var tmpRevision;
    var tmpSize;
    for (var i = data.length - 1; i >= 0; i--) {
        tmpRevision = data[i].LatestRevision;
        tmpSize = data[i].SizeInBytes;

        if (tmpRevision > highestRevision) {
            highestRevision = tmpRevision;
            largestRevisionRepo = data[i];
        }

        if (tmpSize > highestSize) {
            highestSize = tmpSize;
            largestSizeRepo = data[i];
        }
    }

    if (largestRevisionRepo != null) {
        $('#summary-toprevisions').text(largestRevisionRepo.Name);
    } else {
        $('#summary-toprevisions').text("(not available)");
    }

    if (largestSizeRepo != null) {
        $('#summary-topspace').text(largestSizeRepo.Name);
    } else {
        $('#summary-topspace').text("(not available)");
    }
}

jQuery.fn.dataTableExt.oApi.fnReloadAjax = function (oSettings, sNewSource, fnCallback, bStandingRedraw) {
    // DataTables 1.10 compatibility - if 1.10 then `versionCheck` exists.
    // 1.10's API has ajax reloading built in, so we use those abilities
    // directly.
    if (jQuery.fn.dataTable.versionCheck) {
        var api = new jQuery.fn.dataTable.Api(oSettings);

        if (sNewSource) {
            api.ajax.url(sNewSource).load(fnCallback, !bStandingRedraw);
        }
        else {
            api.ajax.reload(fnCallback, !bStandingRedraw);
        }
        return;
    }

    if (sNewSource !== undefined && sNewSource !== null) {
        oSettings.sAjaxSource = sNewSource;
    }

    // Server-side processing should just call fnDraw
    if (oSettings.oFeatures.bServerSide) {
        this.fnDraw();
        return;
    }

    this.oApi._fnProcessingDisplay(oSettings, true);
    var that = this;
    var iStart = oSettings._iDisplayStart;
    var aData = [];

    this.oApi._fnServerParams(oSettings, aData);

    oSettings.fnServerData.call(oSettings.oInstance, oSettings.sAjaxSource, aData, function (json) {
        /* Clear the old information from the table */
        that.oApi._fnClearTable(oSettings);

        /* Got the data - add it to the table */
        var aData = (oSettings.sAjaxDataProp !== "") ?
            that.oApi._fnGetObjectDataFn(oSettings.sAjaxDataProp)(json) : json;

        for (var i = 0 ; i < aData.length ; i++) {
            that.oApi._fnAddData(oSettings, aData[i]);
        }

        oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();

        that.fnDraw();

        if (bStandingRedraw === true) {
            oSettings._iDisplayStart = iStart;
            that.oApi._fnCalculateEnd(oSettings);
            that.fnDraw(false);
        }

        that.oApi._fnProcessingDisplay(oSettings, false);

        /* Callback user function - for event handlers etc */
        if (typeof fnCallback == 'function' && fnCallback !== null) {
            fnCallback(oSettings);
        }
    }, oSettings);
};

jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "file-size-pre": function (a) {
        var x = a.substring(0, a.length - 2);
 
        var x_unit = (a.substring(a.length - 2, a.length) == "MB" ?
            1000 : (a.substring(a.length - 2, a.length) == "GB" ? 1000000 : 1));
 
        return parseInt(x * x_unit, 10);
    },
 
    "file-size-asc": function (a, b) {
        return ((a < b) ? -1 : ((a > b) ? 1 : 0));
    },
 
    "file-size-desc": function (a, b) {
        return ((a < b) ? 1 : ((a > b) ? -1 : 0));
    }
});