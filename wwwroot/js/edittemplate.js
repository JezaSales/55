var Q;

var contentEdit = () => {
    $('.editable_content').dblclick(function () {
        $('.editable_content').not(this).removeAttr('contenteditable').removeAttr('style');
        $('.btn-saveDraft').remove();
        let element = $(this);
        iscontentEditable = element.attr('contenteditable');
        if (!iscontentEditable) {
            element.attr('contenteditable', true)
                .addClass('tinyMCE-editor')
                .css({ 'border': '1px dashed', 'padding': '5px' })
                .after('<button class="btn btn-dark btn-saveDraft" style="position: absolute;height: 38px;">Save Draft</button>');
            setTimeout(function () {
                element.find('.content-overlay').remove();
                element.find('.content-details').remove();
                Q.htmlEditor({
                    selector: '.tinyMCE-editor'
                })
            }, 300);
        }
    });
}
var editDone = () => {
    $('body').on('click', '.btn-saveDraft', function () {
        let element = $(this);
        let parent = $('.editable_content');
        parent.removeAttr('contenteditable')
            .removeClass('tinyMCE-editor')
            .removeAttr('style')
        parent.append(`<div class="content-overlay"></div><div class="content-details fadeIn-bottom">
                                                        <h3 class="content-title">This is a title</h3>
                                                        <p class="content-text">This is a short description</p>
                                                    </div>`);
        element.remove();
    });
}
(() => { contentEdit(), editDone() })();


//(Q => {
//    Q.removeEditor = () => {
//        if (tinymce.editors.length > 0) {
//            tinymce.remove('textarea');
//            tinymce.execCommand('mceRemoveEditor', true, 'textarea');
//        }
//    };

//    Q.initEditor = (options) => {
//        let settings = $.extend({
//            customeDropdown: {
//                enable: false,
//                dropdownOption: [],
//                onselect: function (e) {
//                }
//            },

//        }, options);
//        /* Custome Drodown in Editor*/
//        var customeDropDown = function (editor) {
//            editor.addButton('customeDropdown', {
//                type: 'listbox',
//                text: 'Select',
//                icon: false,
//                onselect: settings.customeDropdown.onselect,
//                values: settings.customeDropdown.dropdownOption,
//                /*onselect: function (e) {
//                    tinyMCE.execCommand(this.value());
//                    tinyMCE.execCommand('mceInsertContent', false, this.value());
//                },
//                 onPostRender: function() {
//                    //Select the firts item by default
//                    this.value('JustifyLeft');
//                 }*/
//            });
//        };
//        /*  End */

//        var initImageUpload = function (editor) {
//            var inp = $('<input id="tinymce-uploader" type="file" name="pic" accept="image/*" style="display:none">');
//            $(editor.getElement()).parent().append(inp);
//            editor.addButton('imageupload', {
//                text: '',
//                icon: 'image',
//                onclick: function (e) {
//                    inp.trigger('click');
//                }
//            });
//            inp.on("change", function (e) {
//                uploadFile($(this), editor);
//            });
//        };

//        var uploadFile = function (inp, editor) {
//            if (inp.val() !== undefined && inp.val() !== '') {
//                var input = inp.get(0);
//                var data = new FormData();
//                data.append('file', input.files[0]);
//                $.ajax({
//                    url: '/uploadTinyMCEImage',
//                    type: 'POST',
//                    data: data,
//                    processData: false,
//                    contentType: false,
//                    success: function (data) {
//                        editor.insertContent('<img class="content-img" src="' + data + '"/>');
//                    },
//                    error: function (jqXHR, textStatus, errorThrown) {
//                        if (jqXHR.responseText) {
//                            errors = JSON.parse(jqXHR.responseText).errors
//                            alert('Error uploading image: ' + errors.join(", ") + '. Make sure the file is an image and has extension jpg/jpeg/png.');
//                        }
//                    }
//                });
//            }
//        };

//        if (tinymce.editors.length > 0) {
//            tinymce.remove(options.selector ?? 'textarea');
//        }
//        tinymce.init({
//            selector: options.selector ?? 'textarea',
//            height: 400,
//            theme: 'modern',
//            inline: true,
//            //plugins: ['advlist autolink lists link image charmap print preview hr anchor pagebreak importcss',
//            //    'searchreplace wordcount visualblocks visualchars code codesample fullscreen',
//            //    'insertdatetime media nonbreaking save table contextmenu directionality',
//            //    'emoticons template paste textcolor colorpicker textpattern imagetools'
//            //],
//            plugins: ['print preview paste importcss searchreplace autolink autosave save directionality code visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap emoticons'],
//            toolbar: 'insertfile undo redo  |fontselect  fontsizeselect forecolor backcolor bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent table imageupload | preview code | customeDropdown',
//            setup: function (editor) {
//                initImageUpload(editor);
//                if (settings.customeDropdown.enable)
//                    customeDropDown(editor);
//            },
//            image_advtab: true,
//            image_caption: true,
//            //quickbars_selection_toolbar: 'bold italic | quicklink h2 h3 blockquote quickimage quicktable',
//            noneditable_noneditable_class: "mceNonEditable",
//            toolbar_mode: 'sliding',
//            contextmenu: "link image imagetools table",
//            templates: [
//                { title: 'Test template 1', content: 'Test 1' },
//                { title: 'Test template 2', content: 'Test 2' }
//            ],
//            content_css: ['//www.tinymce.com/css/codepen.min.css'
//            ]
//        });
//    };

//    Q.htmlEditor = (options) => {
//        let settings = $.extend({
//            CustomeDropdown: {
//                enable: false,
//                DropdownOption: [],
//                onselect: function (e) {
//                }
//            },
//        }, options);
//        Q.initEditor(settings);
//        $(document).on('focusin', function (e) {
//            if ($(e.target).closest(".mce-window").length) {
//                e.stopImmediatePropagation();
//            }
//            if ($(e.target).closest(".mce-window, .moxman-window").length) {
//                e.stopImmediatePropagation();
//            }
//        });
//        //import('/lib/TinyMCE/tinymce.min.js')
//        //    .then(obj => {
//        //        Q.initEditor(settings);
//        //        $(document).on('focusin', function (e) {
//        //            if ($(e.target).closest(".mce-window").length) {
//        //                e.stopImmediatePropagation();
//        //            }
//        //            if ($(e.target).closest(".mce-window, .moxman-window").length) {
//        //                e.stopImmediatePropagation();
//        //            }
//        //        });
//        //    })
//        //    .catch(err => {
//        //        console.log('loading error, no such module exists/n', err);
//        //    });
//    };
//})(Q || (Q = {}));

$(document).ready(function () {
    var owl = $('.owl-carousel');
    owl.owlCarousel({
        margin: 10,
        nav: true,
        loop: true,
        responsive: {
            0: {
                items: 1
            },
            600: {
                items: 2
            },
            1000: {
                items: 3
            }
        }
    })
});

document.addEventListener("scroll", handleScroll);
// get a reference to our predefined button
var scrollToTopBtn = document.querySelector(".myBtn");

function handleScroll() {
    var scrollableHeight = document.documentElement.scrollHeight - document.documentElement.clientHeight;
    var GOLDEN_RATIO = 0.5;

    if ((document.documentElement.scrollTop / scrollableHeight) > GOLDEN_RATIO) {
        //show button
        scrollToTopBtn.style.display = "block";
    } else {
        //hide button
        scrollToTopBtn.style.display = "none";
    }
}

scrollToTopBtn.addEventListener("click", scrollToTop);

function scrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: "smooth"
    });
}


$(document).on('click', 'a[href^="#"]', function (event) {
    event.preventDefault();

    $('html, body').animate({
        scrollTop: $($.attr(this, 'href')).offset().top
    }, 500);
});