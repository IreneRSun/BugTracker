// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// for previewing uploaded image on profile page
function previewImage() {
    const input = document.getElementById('image-file');
    const avatar = document.getElementById('avatar');

    // set up file reader
    const reader = new FileReader();
    reader.onload = () => {
        avatar.setAttribute('src', reader.result);
    }

    // read the file input, if a file was selected
    if (input.files) {
        const selectedFile = input.files[0];
        if (selectedFile) {
            reader.readAsDataURL(selectedFile);
        }
    }
}