/**
 *	Neon Register Script
 *
 *	Developed by Arlind Nushi - www.laborator.co
 */

var neonInviteRegister = neonInviteRegister || {};

; (function ($, window, undefined) {
    "use strict";

    $(document).ready(function () {
        neonInviteRegister.$container = $("#form_invite_register");
        neonInviteRegister.$steps = neonInviteRegister.$container.find(".form-steps");
        neonInviteRegister.$steps_list = neonInviteRegister.$steps.find(".step");
        neonInviteRegister.step = 'step-1'; // current step


        neonInviteRegister.$container.validate({
            rules: {
                name: {
                    required: true
                },

                email: {
                    required: true,
                    email: true
                },

                password: {
                    required: true
                },

            },

            messages: {

                email: {
                    email: 'Invalid E-mail.'
                }
            },

            highlight: function (element) {
                $(element).closest('.input-group').addClass('validate-has-error');
            },


            unhighlight: function (element) {
                $(element).closest('.input-group').removeClass('validate-has-error');
            },

            submitHandler: function (ev) {
                $(".login-page").addClass('logging-in');

                // We consider its 30% completed form inputs are filled
                neonInviteRegister.setPercentage(30, function () {
                    // Lets move to 98%, meanwhile ajax data are sending and processing
                    neonInviteRegister.setPercentage(98, function () {
                        var d = {
                            firstName: $('#firstName').val(),
                            lastName: $('#lastName').val(),
                            emailAddress: $('#emailAddress').val(),
                            password: $('#password').val()
                        };
                        // Send data to the server
                        $.ajax({
                            url: '/register/create',
                            method: 'POST',
                            dataType: 'json',
                            data: d,
                            error: function (e) {
                                alert("An error occoured!" + e);
                            },
                            success: function (response) {
                                if (response.success) {
                                    neonInviteRegister.setPercentage(100);


                                    // We will give some time for the animation to finish, then execute the following procedures	
                                    setTimeout(function () {
                                        // Hide the description title
                                        $(".login-page .login-header .description").slideUp();

                                        // Hide the register form (steps)
                                        neonInviteRegister.$steps.slideUp('normal', function () {
                                            // Remove loging-in state
                                            $(".login-page").removeClass('logging-in');

                                            // Now we show the success message
                                            $(".form-register-success").slideDown('normal');

                                            // You can use the data returned from response variable
                                        });

                                    }, 1000);
                                } else {
                                    // DoSomethingElse()
                                    alert(response.responseText);
                                }

                            }
                        });
                    });
                });
            }
        });

        // Steps Handler
        neonInviteRegister.$steps.find('[data-step]').on('click', function (ev) {
            ev.preventDefault();

            var $current_step = neonInviteRegister.$steps_list.filter('.current'),
                next_step = $(this).data('step'),
                validator = neonInviteRegister.$container.data('validator'),
                errors = 0;

            neonInviteRegister.$container.valid();
            errors = validator.numberOfInvalids();

            if (errors) {
                validator.focusInvalid();
            }
            else {
                var $next_step = neonInviteRegister.$steps_list.filter('#' + next_step),
                    $other_steps = neonInviteRegister.$steps_list.not($next_step),

                    current_step_height = $current_step.data('height'),
                    next_step_height = $next_step.data('height');

                TweenMax.set(neonInviteRegister.$steps, { css: { height: current_step_height } });
                TweenMax.to(neonInviteRegister.$steps, 0.6, { css: { height: next_step_height } });

                TweenMax.to($current_step, .3, {
                    css: { autoAlpha: 0 }, onComplete: function () {
                        $current_step.attr('style', '').removeClass('current');

                        var $form_elements = $next_step.find('.form-group');

                        TweenMax.set($form_elements, { css: { autoAlpha: 0 } });
                        $next_step.addClass('current');

                        $form_elements.each(function (i, el) {
                            var $form_element = $(el);

                            TweenMax.to($form_element, .2, { css: { autoAlpha: 1 }, delay: i * .09 });
                        });

                        setTimeout(function () {
                            $form_elements.add($next_step).add($next_step).attr('style', '');
                            $form_elements.first().find('input').focus();

                        }, 1000 * (.5 + ($form_elements.length - 1) * .09));
                    }
                });
            }
        });

        neonInviteRegister.$steps_list.each(function (i, el) {
            var $this = $(el),
                is_current = $this.hasClass('current'),
                margin = 20;

            if (is_current) {
                $this.data('height', $this.outerHeight() + margin);
            }
            else {
                $this.addClass('current').data('height', $this.outerHeight() + margin).removeClass('current');
            }
        });


        // Login Form Setup
        neonInviteRegister.$body = $(".login-page");
        neonInviteRegister.$login_progressbar_indicator = $(".login-progressbar-indicator h3");
        neonInviteRegister.$login_progressbar = neonInviteRegister.$body.find(".login-progressbar div");

        neonInviteRegister.$login_progressbar_indicator.html('0%');

        if (neonInviteRegister.$body.hasClass('login-form-fall')) {
            var focus_set = false;

            setTimeout(function () {
                neonInviteRegister.$body.addClass('login-form-fall-init')

                setTimeout(function () {
                    if (!focus_set) {
                        neonInviteRegister.$container.find('input:first').focus();
                        focus_set = true;
                    }

                }, 550);

            }, 0);
        }
        else {
            neonInviteRegister.$container.find('input:first').focus();
        }


        // Functions
        $.extend(neonInviteRegister, {
            setPercentage: function (pct, callback) {
                pct = parseInt(pct / 100 * 100, 10) + '%';

                // Normal Login
                neonInviteRegister.$login_progressbar_indicator.html(pct);
                neonInviteRegister.$login_progressbar.width(pct);

                var o = {
                    pct: parseInt(neonInviteRegister.$login_progressbar.width() / neonInviteRegister.$login_progressbar.parent().width() * 100, 10)
                };

                TweenMax.to(o, .7, {
                    pct: parseInt(pct, 10),
                    roundProps: ["pct"],
                    ease: Sine.easeOut,
                    onUpdate: function () {
                        neonInviteRegister.$login_progressbar_indicator.html(o.pct + '%');
                    },
                    onComplete: callback
                });
            }
        });
    });

})(jQuery, window);