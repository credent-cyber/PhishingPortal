
    $('.summernote').summernote({
        oninit: function () {
            $("div.note-editor button[data-event='codeview']").click();
        }
    });
