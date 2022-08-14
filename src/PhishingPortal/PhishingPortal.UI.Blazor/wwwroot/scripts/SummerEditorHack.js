
function summernoteEditorHack(code, selector) {
    let $editor = $(selector);
    if ($editor !== undefined)
        $editor.summernote('code', code);
}

function getSummernoteEditorContent(selector) {
    let $editor = $(selector);

    if ($editor !== undefined)
        return $editor.summernote('code');
}