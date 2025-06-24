GoBangladesh.Employee = {
    GetAllEmployees: ''
};

GoBangladesh.Employee.GetAllEmployees = function (id, dimmerId) {
    GoBangladesh.Datables.ShowDimmer(dimmerId);
    var component = '#' + id;
    $(component).DataTable();

    appClient.get('/client/getall', null,
        function (response) {
            GoBangladesh.Employee.ShowAll(response.data, component, dimmerId);
        })
}
GoBangladesh.Employee.ShowAll = function (data, component, dimmerId) {
    $(component).dataTable().fnDestroy();
    GoBangladesh.Employee.table = $(component).DataTable({
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
            { "data": "email", "name": "Email", "autoWidth": true },
            { "data": "country", "name": "Country", "autoWidth": true },
            { "data": "birthDate", "name": "Birth Day", "autoWidth": true },
            { "data": "paymentOption", "name": "Payment", "autoWidth": true },
            { "data": "costPerMinute", "name": "Cost/Minute", "autoWidth": true },
            { "data": "moneyLent", "name": "Money Lent", "autoWidth": true },
            { "data": "rating", "name": "Rating", "autoWidth": true },
            {
                "render": function (data, type, full, meta) {
                    var btn = "<a title='Edit' class='label label-info icon-left update' onclick=GoBangladesh.Client.Edit('" + encodeURIComponent(full.id) + "') ><i class='entypo-pencil'></i>Edit</a>";
                    btn = btn + "<a class='label label-danger icon-left delete'  onclick=DeleteEntity('" + encodeURIComponent(full.id) + "','Client','" + component + "')> <i class='entypo-cancel'></i>Delete</a>";
                    return btn;
                }
            }
        ]
    });
    GoBangladesh.Datables.HideDimmer(dimmerId);
    GoBangladesh.Datables.SetDdl(component);
}

GoBangladesh.Employee.Edit = function (id) {
    $('#entityId').val(id);

    appClient.get('/client/get/' + id, null,
        function (response) {
            if (response) {
                var data = response.data;
                $('#name').val(data.name);
                $('#userName').val(data.userName);
                $('#email').val(data.email);
                $('#category').val(data.category);
                $('#country').val(data.country);
                $('#dob').val(data.dob);
                $('#paymentOption').val(data.paymentOption);
                $('#costperminute').val(data.costperminute);
                $('#moneyLent').val(data.moneyLent);
                $('#rating').val(data.rating);
                $('#note').val(data.note);

                jQuery.noConflict();
                $('#Client_crud_modal').modal('show');
            }
            else {
                GoBangladesh.Settings.Toast('Error', 'An error occured on Getting Client Details', 'error');
            }
        })
}

$("#Client_crud_frm").submit(function (e) {
    e.preventDefault();
    var id = $('#entityId').val();

    var msg = 'create';
    var api = '';
    if (id === '') {
        api = '/client/create';
    } else {
        msg = 'update';
        api = '/client/update';
    }

    var name = $("#name").val();
    var userName = $("#userName").val();
    var email = $("#email").val();
    var category = $("#category").val();
    var country = $("#country").val();
    var dob = $("#dob").val();
    var paymentOption = $("#paymentOption").val();
    var acostperminuteme = $("#costperminute").val();
    var moneyLent = $("#moneyLent").val();
    var rating = $("#rating").val();
    var note = $("#note").val();

    appClient.post(api, {
        id: id,
        name: name,
        userName: userName,
        email: email,
        category: category,
        country: country,
        dob: dob,
        paymentOption: paymentOption,
        acostperminuteme: acostperminuteme,
        moneyLent: moneyLent,
        rating: rating,
        note: note
    },
        function (response) {
            if (response.isSuccess) {
                GoBangladesh.Settings.Toast('Success', 'Client  ' + msg + ' has been Succeed', 'Success');
                $('#Client_crud_modal').modal('hide');
                GoBangladesh.Settings.ReloadDt();
            }
            else {
                GoBangladesh.Settings.Toast('Error', 'Client ' + msg + ' has been Succeedl', 'error');
                $('#Client_crud_modal').modal('hide');
                GoBangladesh.Settings.ReloadDt();
            }
        })
});

GoBangladesh.Employee.ResetEmployeeForm = function () {
    $('#entityId').val('');
    $("#name").val('');
    $("#userName").val('');
    $("#email").val('');
    $("#category").val('');
    $("#country").val('');
    $("#dob").val('');
    $("#paymentOption").val('');
    $("#costperminute").val('');
    $("#moneyLent").val('');
    $("#rating").val('');
    $("#note").val('');
};

GoBangladesh.Employee.Add = function (id) {
    GoBangladesh.Employee.ResetClientForm();

    jQuery.noConflict();
    $('#Client_crud_modal').modal('show');
}