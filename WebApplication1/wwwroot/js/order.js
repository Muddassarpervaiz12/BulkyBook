var dataTable;
$(document).ready(function () {
    //get url search righ now on window
    var url = window.location.search;
    if (url.includes("inprocess"))
    {
        loadDataTable("inprocess");
    }
    else
    {
        if (url.includes("completed"))
        {
            loadDataTable("completed");
        }
        else {
            if (url.includes("pending"))
            {
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
//table function load a table in which have different style
function loadDataTable(stauts) {
    dataTable = $('#tblData').DataTable(
        {
            "ajax": {
                //here we get status if status is inprocess display inprocess data and so on
                "url": "/Admin/Order/GetAll?status=" + stauts
            },
            "columns": [
                { "data": "id", "width": "5%" },
                { "data": "name", "width": "10%" },
                { "data": "phoneNumber", "width": "15%" },
                { "data": "applicationUser.email", "width": "15%" },
                { "data": "orderStatus", "width": "10%" },
                { "data": "orderTotal", "width": "10%" },
                {
                    "data": "id",
                    "render": function (data) {
                        return `
                            
                           <a type="submit" class="btn btn-success" href="/Admin/Order/Details?orderId=${data}" style="width:80px">
                            Details
                        </a> &nbsp
                        
                          `
                    },
                    "width": "10%" 
                },
            ]

        });
}