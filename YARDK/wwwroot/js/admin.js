(function ($) {
    "use strict";

    // Spinner
    var spinner = function () {
        setTimeout(function () {
            if ($('#spinner').length > 0) {
                $('#spinner').removeClass('show');
            }
        }, 1);
    };
    spinner();
    
    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({scrollTop: 0}, 500, 'easeInOutExpo');
        return false;
    });

    // Sidebar Toggler - For Mobile View
    $('.sidebar-toggler').click(function () {
        $('.sidebar, .content').toggleClass('open');
        return false;
    });
    
    // Initialize sidebar based on stored state
    $(document).ready(function() {
        // Add text spans to navigation links for mini sidebar mode
        $('.sidebar .navbar-nav .nav-link').each(function() {
            const $this = $(this);
            // Check if the link has already been processed
            if ($this.find('.nav-text').length === 0) {
                const icon = $this.find('i');
                const text = $this.text().trim();
                $this.empty();
                $this.append(icon);
                $this.append('<span class="nav-text ms-2">' + text + '</span>');
            }
        });
    });

    // Tooltip initialization
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    // Initialize data tables if present
    if($.fn.DataTable) {
        $('.data-table').DataTable({
            responsive: true
        });
    }

    // Handle confirmation dialogs
    $('.confirm-action').click(function(e) {
        // استخدام الرسالة المناسبة حسب اللغة الحالية
        const lang = window.currentLang || 'en';
        
        // البحث عن رسالة التأكيد حسب اللغة
        let confirmMessage;
        if ($(this).data('confirm-' + lang)) {
            // إذا كانت هناك رسالة محددة للغة الحالية
            confirmMessage = $(this).data('confirm-' + lang);
        } else if ($(this).data('confirm')) {
            // استخدام رسالة التأكيد العامة إذا كانت موجودة
            confirmMessage = $(this).data('confirm');
        } else {
            // رسالة افتراضية بناءً على اللغة
            confirmMessage = lang === 'ar' ? 'هل أنت متأكد من تنفيذ هذا الإجراء؟' : 'Are you sure you want to perform this action?';
        }
        
        if (!confirm(confirmMessage)) {
            e.preventDefault();
            return false;
        }
        return true;
    });

    // Form validation feedback
    (function() {
        'use strict';
        
        // Fetch all forms that need validation
        var forms = document.querySelectorAll('.needs-validation');
        
        // Loop over and prevent submission
        Array.prototype.slice.call(forms).forEach(function(form) {
            form.addEventListener('submit', function(event) {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                
                form.classList.add('was-validated');
            }, false);
        });
    })();

    // Image preview for file inputs
    $('.image-preview-input').on('change', function() {
        var input = this;
        var previewElement = $($(this).data('preview'));
        
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            
            reader.onload = function(e) {
                previewElement.attr('src', e.target.result).show();
            }
            
            reader.readAsDataURL(input.files[0]);
        }
    });
    
    // Language Switcher
    $(document).ready(function() {
        // Check if language preference is stored
        const currentLang = localStorage.getItem('admin-language') || 'ar';
        updateLanguageUI(currentLang);
        
        // Set global variable for language access by charts
        window.currentLang = currentLang;
        
        // Handle language change
        $('.dropdown-item[data-lang]').on('click', function(e) {
            e.preventDefault();
            const lang = $(this).data('lang');
            localStorage.setItem('admin-language', lang);
            updateLanguageUI(lang);
            applyLanguage(lang);
        });
        
        // Apply stored language on page load
        applyLanguage(currentLang);
        
        // Function to update language UI
        function updateLanguageUI(lang) {
            const langText = lang === 'ar' ? 'العربية' : 'English';
            $('.current-lang').text(langText);
            
            // Highlight active language
            $('.dropdown-item[data-lang]').removeClass('active');
            $(`.dropdown-item[data-lang="${lang}"]`).addClass('active');
        }
        
        // Function to apply language
        function applyLanguage(lang) {
            if (lang === 'ar') {
                $('body').addClass('rtl').attr('dir', 'rtl');
                $('.ar-text').removeClass('d-none');
                $('.en-text').addClass('d-none');
                
                // Apply Arabic font for body
                $('body').css('font-family', "'Cairo', sans-serif");
            } else {
                $('body').removeClass('rtl').attr('dir', 'ltr');
                $('.en-text').removeClass('d-none');
                $('.ar-text').addClass('d-none');
                
                // Apply English font for body
                $('body').css('font-family', "'Open Sans', sans-serif");
            }
            
            // Update global language variable
            window.currentLang = lang;
            
            // Trigger event for other components to react
            $(document).trigger('languageChanged', [lang]);
            
            // Add fade effect
            $('body').addClass('fade-transition');
            $('body').addClass('fade-out');
            
            setTimeout(function() {
                $('body').removeClass('fade-out');
                $('body').addClass('fade-in');
            }, 50);
            
            setTimeout(function() {
                $('body').removeClass('fade-transition');
                $('body').removeClass('fade-in');
            }, 300);
        }
    });

})(jQuery); 