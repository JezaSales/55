var Q;
(Q => {
    Q.removeEditor = (selector) => {
        tinymce.remove(selector);
        //if (tinymce.editors.length > 0) {
        //    tinymce.remove(selector);
        //    tinymce.execCommand('mceRemoveEditor', true, selector ?? 'textarea');
        //}
    };

    Q.initEditor = (options) => {
        let settings = $.extend({
            customeDropdown: {
                enable: false,
                dropdownOption: [],
                onselect: function (e) {
                }
            },

        }, options);
        /* Custome Drodown in Editor*/
        var customeDropDown = function (editor) {
            editor.addButton('customeDropdown', {
                type: 'listbox',
                text: 'Select',
                icon: false,
                onselect: settings.customeDropdown.onselect,
                values: settings.customeDropdown.dropdownOption,
                /*onselect: function (e) {
                    tinyMCE.execCommand(this.value());
                    tinyMCE.execCommand('mceInsertContent', false, this.value());
                },
                 onPostRender: function() {
                    //Select the firts item by default
                    this.value('JustifyLeft');
                 }*/
            });
        };
        /*  End */

        var initImageUpload = function (editor) {
            var inp = $('<input id="tinymce-uploader" type="file" name="pic" accept="image/*" style="display:none">');
            $(editor.getElement()).parent().append(inp);
            editor.addButton('imageupload', {
                text: '',
                icon: 'image',
                onclick: function (e) {
                    inp.trigger('click');
                }
            });
            inp.on("change", function (e) {
                uploadFile($(this), editor);
            });
        };

        var uploadFile = function (inp, editor) {
            if (inp.val() !== undefined && inp.val() !== '') {
                var input = inp.get(0);
                var data = new FormData();
                data.append('file', input.files[0]);
                $.ajax({
                    url: '/uploadTinyMCEImage',
                    type: 'POST',
                    data: data,
                    processData: false,
                    contentType: false,
                    success: function (data) {
                        editor.insertContent('<img class="content-img" src="' + data + '"/>');
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        if (jqXHR.responseText) {
                            errors = JSON.parse(jqXHR.responseText).errors
                            alert('Error uploading image: ' + errors.join(", ") + '. Make sure the file is an image and has extension jpg/jpeg/png.');
                        }
                    }
                });
            }
        };

        if (tinymce.editors.length > 0) {
            tinymce.remove('textarea');
        }
        tinymce.init({
            selector: options.selector ?? 'textarea',
            height: 400,
            theme: 'modern',
            //plugins: ['advlist autolink lists link image charmap print preview hr anchor pagebreak importcss',
            //    'searchreplace wordcount visualblocks visualchars code codesample fullscreen',
            //    'insertdatetime media nonbreaking save table contextmenu directionality',
            //    'emoticons template paste textcolor colorpicker textpattern imagetools'
            //],
            plugins: ['print preview paste importcss searchreplace autolink autosave save directionality code visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap emoticons'],
            toolbar: 'insertfile undo redo  |fontselect  fontsizeselect forecolor backcolor bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent table imageupload | preview code | customeDropdown',
            setup: function (editor) {
                initImageUpload(editor);
                if (settings.customeDropdown.enable)
                    customeDropDown(editor);
            },
            image_advtab: true,
            image_caption: true,
            //quickbars_selection_toolbar: 'bold italic | quicklink h2 h3 blockquote quickimage quicktable',
            noneditable_noneditable_class: "mceNonEditable",
            toolbar_mode: 'sliding',
            contextmenu: "link image imagetools table",
            templates: [
                { title: 'Test template 1', content: 'Test 1' },
                { title: 'Test template 2', content: 'Test 2' }
            ],
            content_css: ['//www.tinymce.com/css/codepen.min.css'
            ]
        });
    };

    Q.htmlEditor = (options) => {
        let settings = $.extend({
            CustomeDropdown: {
                enable: false,
                DropdownOption: [],
                onselect: function (e) {
                }
            },
        }, options);

        import('/lib/TinyMCE/tinymce.min.js')
            .then(obj => {
                Q.initEditor(settings);
                $(document).on('focusin', function (e) {
                    if ($(e.target).closest(".mce-window").length) {
                        e.stopImmediatePropagation();
                    }
                    if ($(e.target).closest(".mce-window, .moxman-window").length) {
                        e.stopImmediatePropagation();
                    }
                });
            })
            .catch(err => console.log('loading error, no such module exists/n', err));
    };
})(Q || (Q = {}));