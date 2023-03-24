var dataTable;
$(document).ready(function () {
    loadDataTable();
});
//table function load a table in which have different style
function loadDataTable() {
    dataTable = $('#tblData').DataTable(
        {
            "ajax": {
                "url": "/Admin/Product/GetAll"
            },
            "columns": [
                { "data": "title", "width": "15%" },
                { "data": "isbn", "width": "10%" },
                { "data": "price", "width": "10%" },
                { "data": "author", "width": "15%" },
                { "data": "catogery.name", "width": "15%" },
                { "data": "coverType.name", "width": "15%" },
                {
                    "data": "id",
                    "render": function (data) {
                        return `
                            
                           <a type="submit" class="btn btn-success" href="/Admin/Product/Upsert?id=${data}" style="width:80px">
                            Edit
                        </a> &nbsp
                        <a onClick=Delete('/Admin/Product/Delete/${data}') class="btn btn-primary" style="width:80px">
                            Delete
                        </a>
                        
                          `
                    },
                    "width": "20%" 
                },
            ]

        });
}

//delete function
function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
          $.ajax({
                    url: url,
                    type: 'Delete',
                    success: function (data) {
                        if (data.success) {
                            dataTable.ajax.reload();
                            toastr.success(data.message);
                        }
                        else {
                            toastr.error(data.message);
                        }
                    }
                    })
            
        }
    })
}