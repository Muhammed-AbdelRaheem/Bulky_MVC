var dataTable;

$(document).ready(function () {
    var url = window.location.search;

    if (url.includes("inprocess")) {
        loadDataTable("inprocess");

    }
    else {
        if (url.includes("completed")) {
            loadDataTable("completed");

        }
        else {
            if (url.includes("pending")) {
                loadDataTable("pending");

            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");

                }
                else {
                    loadDataTable("all");

                }
            }
        }
    }


});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/order/getall?status='+ status },
        "columns": [
            { data: 'id', "width": "5%", className: "text-center" },
            { data: 'name', "width": "15%", className: "text-center" },
            { data: 'phoneNumber', "width": "20%", className: "text-center" },
            { data: 'applicationUser.email', "width": "15%", className: "text-center" },
            { data: 'orderStatus', "width": "10%", className: "text-center" },
            { data: 'orderTotal', "width": "10%", className: "text-center" },

            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group text-center" role="group">
                     <a href="/admin/Order/Details?OrderId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> </a>               
                                
                     </div>`
                },
                "width": "25%",
                 className: "text-center"
            }
        ]
    });
}

