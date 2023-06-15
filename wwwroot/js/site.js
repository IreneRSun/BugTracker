// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(() => {
    // show search results, if any
    $(".search-results").modal("show");
    // handle dropdown search
    $(".search2-multiple").select2();
});

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
        if (fileSizeMB > 16) {
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
