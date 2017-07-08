$(function () {

    $('.download-btn').click(function () {

        //Copy code into hidden field
        var editor = ace.edit($(".pad-edit-textbox")[0]);
        $('#codeform input[name=code]').val(editor.session.getValue());

        //Post form
        $('#codeform').submit();
    });

    $('.newpkg-btn').click(function () {
        $('#downloadNotes').hide(500);
        $('#createPackage').show(500);
        setTimeout(function () {
            Codepad.all();
        }, 500);
        
    })

    if ($('.download-btn:visible').length > 0)
    {
        Codepad.all();
    }

    if ($('#download-link').length > 0) {
        setTimeout(function () {
            window.location = $('#download-link').val();
        }, 1500);        
    }
});