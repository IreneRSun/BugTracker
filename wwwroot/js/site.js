// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Function that shows the loading screen
function showLoading() {
    $(".load-screen").addClass("visible");
}

// Function that submits a given form
function submitForm(formId) {
    showLoading();
    $(`#${formId}`).submit();
}

// Function that updates the profile image on the profile page to the uploaded image for previewing when called
function previewImage() {
    const fileInput = $("#image-file-upload")
    const inputFiles = fileInput.prop("files");
    const avatar = $("#avatar");
    const sizeWarning = $("#file-size-warning");
    const typeWarning = $("#file-type-warning");

    // set up file reader
    const reader = new FileReader();
    reader.onload = () => {
        avatar.attr("src", reader.result);
    }

    // read the file input or warn the user if a file was selected
    if (inputFiles && inputFiles.length) {
        const selectedFile = inputFiles[0];

        // check the file type
        if (!selectedFile.type.match("image.*")) {
            typeWarning.removeClass("d-none");
            fileInput.val("");
            return;
        } else {
            typeWarning.addClass("d-none");
        }

        // check the file size
        const fileSizeKB = selectedFile.size / (1024);
        if (fileSizeKB >= 64) {
            sizeWarning.removeClass("d-none");
            fileInput.val("");
            return;
        } else {
            sizeWarning.addClass("d-none");
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
        return $(`<div><img src="${result.avatar}" class="avatar-size-30 rounded-circle" /> ${result.name} <div class="text-grey">${result.date}</div> </div>`);
    } else {
        return $(`<div class="py-1"><i class="fa fa-search" aria-hidden="true"></i> Search for a project ...</div>`);
    }
}

function formatProjectSelection(result) {
    if (result.avatar) {
        return $(`<div><img src="${result.avatar}" class="avatar-size-30 rounded-circle" /> ${result.name}</div>`) || result.text;
    } else {
        return $(`<div class="py-1"><i class="fa fa-search" aria-hidden="true"></i> Search for a project ...</div>`);
    }
}

// User search result template (for adding users)
function formatUser(result) {
    if (result.loading) {
        return result.text;
    }

    if (result.avatar) {
        return $(`<div><img src="${result.avatar}" class="avatar-size-30 rounded-circle" /> ${result.name}</div>`);
    } else {
        return $(`<div class="py-1"><i class="fa fa-user-plus" aria-hidden="true"></i> Search for a user to add ...</div>`);
    }
}

function formatUserSelection(result) {
    if (result.avatar) {
        return $(`<div><img src="${result.avatar}" class="avatar-size-30 rounded-circle" /> ${result.name}</div>`) || result.text;
    } else {
        return $(`<div class="py-1"><i class="fa fa-user-plus" aria-hidden="true"></i> Search for a user to add ...</div>`);
    }
}

// Developer select result template
function formatUserFromData(result) {
    if (result.loading) {
        return result.text;
    }

    var data = $(result.element).data();
    if (data.avatar) {
        return $(`<div class="text-black"><img src="${data.avatar}" class="avatar-size-30 rounded-circle" /> ${data.name}</div>`);
    } else {
        return $(`<div class="py-1 text-black">Select an available user to assign to this bug report ...</div>`);
    }
}

function formatUserFromDataSelection(result) {
    var data = $(result.element).data();

    if (data.avatar) {
        return $(`<div><img src="${data.avatar}" class="avatar-size-30 rounded-circle" /> ${data.name}</div>`) || data.text;
    } else {
        return $(`<div class="py-1">Select an available user to assign to this bug report ...</div>`);
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
                    const safeName = item.Name.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
                    return {
                        id: item.ID,
                        name: safeName,
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
                console.log(data);

                var datalist = JSON.parse(data);
                var items = datalist.map(function (item) {
                    const safeName = item.Name.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
                    return {
                        id: item.ID,
                        name: safeName,
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
        placeholder: "Search for a user ...",
        minimumInputLength: 3,
        templateResult: formatUser,
        templateSelection: formatUserSelection
    });

    // Handle developer select
    $(".developer-select").select2({
        templateResult: formatUserFromData,
        templateSelection: formatUserFromDataSelection
    });

});