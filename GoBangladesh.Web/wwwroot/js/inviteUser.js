GoBangladesh.InviteUser = {};

GoBangladesh.Datables.GetAllInvitation = function (id, dimmerId) {
    GoBangladesh.Datables.ShowDimmer(dimmerId);
    var component = '#' + id;
    $(component).DataTable();

    appClient.get('/invite/getall', null,
        function (response) {
            GoBangladesh.Datables.ShowAllInvitation(response, component, dimmerId);
        })
}
GoBangladesh.Datables.ShowAllInvitation = function (data, component, dimmerId) {
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
        "columnDefs":
            [{
                "targets": [0],
                "visible": false,
                "searchable": false
            }
            ],
        "columns": [
            { "data": "id", "name": "Invitation Id", "autoWidth": true },
            { "data": "firstName", "name": "First Name", "autoWidth": true },
            { "data": "lastName", "name": "Last Name", "autoWidth": true },
            { "data": "emailAddress", "name": "Email", "autoWidth": true },
            { "data": "roleName", "name": "Role", "autoWidth": true },
            {
                "render": function (data, type, full, meta) {
                    var btn = "";
                    if (full.status === '2') {
                        btn = "<span class='label label-info'><i class='entypo-lock'></i>Registered</span>";
                    } else if (full.status === '1') {
                        btn = "<span class='label label-secondary'><i class='entypo-direction'></i>Pending</span>";
                    } else if (full.status === '3') {
                        btn = "<span class='label label-success'><i class='entypo-thumbs-up'></i>Approved</span>";
                    }
                    return btn;
                }
            },
            {
                "render": function (data, type, full, meta) {
                    var btn = "";
                    if (full.status === '2') {
                        btn = "<a title='Approve User' class='label label-success icon-left update' onclick=GoBangladesh.InviteUser.Approve('" + encodeURIComponent(full.id) + "') ><i class='entypo-thumbs-up'></i> Approve</a>";
                    }
                    btn = btn + "<a title='Delete' class='label label-danger icon-left delete'  onclick=DeleteEntity('" + encodeURIComponent(full.id) + "','InviteUser','" + component + "')> <i class='entypo-trash'></i></a>";
                    return btn;
                }
            },
        ]
    });
    GoBangladesh.Datables.HideDimmer(dimmerId);
    GoBangladesh.Datables.SetDdl(component);
}

GoBangladesh.InviteUser.Invite = function () {
    GoBangladesh.InviteUser.ResetCrudForm();
    jQuery.noConflict();

    appClient.get('/role/getall', null,
        function (response) {
            for (var i = 0; i < response.data.length; i++) {
                $('#roleid').append('<option value=' + response.data[i].id + '> ' + response.data[i].name + ' </option>');
            }
            $('#User_invite_modal').modal('show');
        })
}

GoBangladesh.InviteUser.ResetCrudForm = function () {
    $("#firstName").val('');
    $("#lastName").val('');
    $('#email').val('');
    $('#roleid').empty();
}

$("#user_invite_frm").submit(function (e) {
    e.preventDefault();

    var firstName = $('#firstName').val();
    var lastName = $('#lastName').val();
    var email = $('#emailAddress').val();
    var roleid = $('#roleid').val();

    if (firstName && email && roleid) {
        appClient.post('/invite/send', {
            firstName: firstName,
            lastName: lastName,
            emailAddress: email,
            roleid: roleid
        },
            function (response) {
                if (response.isSuccess) {
                    GoBangladesh.Settings.Toast('Success', 'User has been Invitted', 'Success');
                }
                else {
                    GoBangladesh.Settings.Toast('Error', 'User Already in Pending State', 'error');
                }

                jQuery.noConflict();
                GoBangladesh.InviteUser.ResetCrudForm();
                $('#User_invite_modal').modal('hide');
                GoBangladesh.Settings.ReloadDt();
            })
    }
    else {
        GoBangladesh.Settings.Toast('Error', 'Mandetory fields should not be empty', 'error');
        return;
    }
});

GoBangladesh.InviteUser.Approve = function (id) {
    jQuery.noConflict();

    appClient.get('/invite/approve/' + id, null,
        function (data) {
            if (data.isSuccess) {
                GoBangladesh.Settings.Toast('Success', 'User has been Approved', 'Success');
            }
            else {
                GoBangladesh.Settings.Toast('Error', 'User approval unsuccessful', 'error');
            }

            jQuery.noConflict();
            GoBangladesh.Settings.ReloadDt();
        })
}