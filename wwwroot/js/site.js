// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Function that submits a given form
function submitForm(formId) {
    const form = document.getElementById(formId);
    form.submit();
}

// Function updates the profile image on the profile page to the uploaded image for previewing when called
function previewImage() {
    const input = document.getElementById("image-file");
    const avatar = document.getElementById("avatar");
    const warning = document.getElementById("file-size-warning");

    // set up file reader
    const reader = new FileReader();
    reader.onload = () => {
        avatar.setAttribute("src", reader.result);
    }

    // read the file input or warn the user if a file was selected
    if (input.files && input.files.length) {
        const selectedFile = input.files[0];

        // check the file size
        const fileSizeMB = selectedFile.size / (1024 * 1024);
        if (fileSizeMB > 5) {
            warning.style.display = "block";
            return;
        } else {
            warning.style.display = "none";
        }

        // read the file
        if (selectedFile) {
            reader.readAsDataURL(selectedFile);
        }
    }
}

// Project search result template
function formatProject(result) {
    if (result.loading) {
        return result.text;
    }

    if (result.avatar) {
        return $(`<div><img src="${result.avatar}" class="custom-avatar-size-1 rounded-circle" /> ${result.name} <br> ${result.date}</div>`);
    } else {
        return $(`<div><i class="fa fa-search" aria-hidden="true"></i> Search for a project ...</div>`);
    }
}

function formatProjectSelection(result) {
    if (result.id) {
        return $(`<div><img src="${result.avatar}" class="custom-avatar-size-1 rounded-circle" /> ${result.name}</div>`) || result.text;
    } else {
        return $(`<div><i class="fa fa-search" aria-hidden="true"></i> Search for a project ...</div>`);
    }
}

// User search result template
function formatUser(result) {
    if (result.loading) {
        return result.text;
    }

    if (result.avatar) {
        return $(`<div><img src="${result.avatar}" class="custom-avatar-size-1 rounded-circle" /> ${result.name}</div>`);
    } else if (result.name) {
        return $(`<div><span class="custom-avatar-size-1 rounded-circle"><i class="fa fa-user-circle-o"></i></span> ${result.name}</div>`);
    } else {
        return $(`<div><i class="fa fa-user-plus" aria-hidden="true"></i> Search for a user ...</div>`);
    }
}

function formatUserSelection(result) {
    if (result.avatar) {
        return $(`<div><img src="${result.avatar}" class="custom-avatar-size-1 rounded-circle" /> ${result.name}</div>`) || result.text;
    } else if (result.name) {
        return $(`<div><span class="custom-avatar-size-1 rounded-circle"><i class="fa fa-user-circle-o"></i></span> ${result.name}</div>`) || result.text;
    } else {
        return $(`<div><i class="fa fa-user-plus" aria-hidden="true"></i> Search for a user ...</div>`);
    }
}

// Actions when document is ready
$(document).ready(function () {
    // Handle project search
    $(".project-search").select2({
        ajax: {
            url: "/AccountActions/SearchProjects",
            type: "GET",
            dataType: "json",
            delay: 250,
            data: function (params) {
                return {
                    search: params.term,
                    page: params.page
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                var datalist = JSON.parse(data);
                var items = datalist.map(function (item) {
                    return {
                        id: item.ID,
                        name: item.Name,
                        date: item.Date,
                        avatar: item.Avatar
                    };
                });

                return {
                    results: items,
                    pagination: {
                        more: (params.page * 30) < data.total_count
                    }
                };
            },
            cache: true
        },
        placeholder: "Search for a project ...",
        minimumInputLength: 3,
        templateResult: formatProject,
        templateSelection: formatProjectSelection
    });

    // Handle user search
    $(".user-search").select2({
        ajax: {
            url: "/AccountActions/SearchUsers",
            dataType: "json",
            delay: 250,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    results: function (data, params) {
                        params.page = params.page || 1;

                        var datalist = JSON.parse(data);
                        var items = datalist.map(function (item) {
                            return {
                                id: item.ID,
                                name: item.Name,
                                avatar: item.Avatar
                            };
                        });

                        return {
                            results: items,
                            pagination: {
                                more: (params.page * 30) < data.total_count
                            }
                        };
                    },
                    pagination: {
                        more: (params.page * 30) < data.total_count
                    }
                };
            },
            cache: true
        },
        placeholder: "Search for a user ...",
        minimumInputLength: 3,
        templateResult: formatUser,
        templateSelection: formatUserSelection
    });

    // Handle developer select
    $(".developer-select").select2();
});
