GoBangladesh.Designation = {
    GetAllDesignations: ''
};

GoBangladesh.Designation.GetAllDesignations = function (id, dimmerId) {
    GoBangladesh.Datables.ShowDimmer(dimmerId);
    var component = '#' + id;
    $(component).DataTable();

    appClient.get('/designations/getall', null,
        function (response) {
            GoBangladesh.Designation.ShowAll(response.data, component, dimmerId);
        })
}

GoBangladesh.Designation.ShowAll = function (data, component, dimmerId) {
    $(component).dataTable().fnDestroy();
    $(component).DataTable({
        //"order": [[1, "asc"]],

        "aLengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        "processing": true,
        "serverSide": false,
        "filter": true,
        "orderMulti": false,
        "bAutoWidth": false,
        "data": data,
        "dom": 'Bfrtip',
        "buttons": [
            'csv', 'excel', 'pdf', 'print'
        ],
        "columnDefs":
            [{
                "targets": [0],
                "visible": false,
                "searchable": false
            }
            ],
        "columns": [

            { "data": "id", "name": "id", "autoWidth": true },
            { "data": "name", "name": "Name", "autoWidth": true },
            { "data": "responsibilities", "name": "Responsibilities", "autoWidth": true },
            {
                "render": function (data, type, full, meta) {
                    var dt = moment(full.createTime).format('DD-MM-YYYY');
                    var btn = btn = "<span><i class='entypo-calendar'></i>" + dt+" </span>";                   
                    return btn;
                }
            },
            { "data": "createdBy", "name": "Created By", "autoWidth": true },
            {
                "render": function (data, type, full, meta) {
                    var btn = "<a title='Edit' class='label label-info icon-left update' onclick=GoBangladesh.Designation.Edit('" + encodeURIComponent(full.id) + "') ><i class='entypo-pencil'></i></a>";
                    btn = btn + "<a title='Delete'  class='label label-danger icon-left delete'  onclick=DeleteEntity('" + encodeURIComponent(full.id) + "','Department','" + component + "')> <i class='entypo-trash'></i></a>";
                    return btn;
                }
            }
        ]
    });
    GoBangladesh.Datables.HideDimmer(dimmerId);
    GoBangladesh.Datables.SetDdl(component);
}

GoBangladesh.Designation.Add = function (id) {
    GoBangladesh.Designation.ResetForm();
    jQuery.noConflict();

    $('#Designation_crud_modal').modal('show');
}

GoBangladesh.Designation.Edit = function (id) {
    $('#entityId').val(id);

    appClient.get('/designations/get/' + id, null,
        function (response) {
            if (response) {
                var data = response.data;
                $('#name').val(data.name);
                $('#responsibilities').val(data.responsibilities);

                jQuery.noConflict();
                $('#Designation_crud_modal').modal('show');
            }
            else {
                GoBangladesh.Settings.Toast('Error', 'An error occured on Getting Designation Details', 'error');
            }
        })
}

$("#Designation_crud_frm").submit(function (e) {
    e.preventDefault();
    var id = $('#entityId').val();
    var name = $("#name").val();
    var responsibilities = $("#responsibilities").val();
    var msg = 'create';
    var api = '';

    if (id === '') {
        appClient.post('/designations/create', {
            name: name,
            responsibilities: responsibilities
        }, function (response) {
            if (response.data.isSuccess) {
                GoBangladesh.Settings.Toast('Success', 'Designation  ' + msg + ' has been Succeed', 'Success');
                $('#Designation_crud_modal').modal('hide');
                GoBangladesh.Settings.ReloadDt();
            }
            else {
                GoBangladesh.Settings.Toast('Error', 'Designation ' + msg + ' has been Failed!', 'error');
                $('#Designation_crud_modal').modal('hide');
                GoBangladesh.Settings.ReloadDt();
            }
        })

    } else {
        msg = 'update';
        appClient.put('/designations/update/' + id, {
            name: name,
            responsibilities: responsibilities
        }, function (response) {
            if (response.data.isSuccess) {
                GoBangladesh.Settings.Toast('Success', 'Designation  ' + msg + ' has been Succeed', 'Success');
                $('#Designation_crud_modal').modal('hide');
                GoBangladesh.Settings.ReloadDt();
            }
            else {
                GoBangladesh.Settings.Toast('Error', 'Designation ' + msg + ' has been Failed!', 'error');
                $('#Designation_crud_modal').modal('hide');
                GoBangladesh.Settings.ReloadDt();
            }
        })
    }
});

GoBangladesh.Designation.ResetForm = function () {
    $('#entityId').val('');
    $("#name").val('');
    $("#responsibilities").val('');
};