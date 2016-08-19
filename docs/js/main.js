$(document).ready(function() {
  $('[rel="external"]').attr("target","_blank");
  
  $(window).bind('scroll', function() {
    ($(window).scrollTop() > 50) ?
      $('#header').addClass('navbar-fixed-top'):
      $('#header').removeClass('navbar-fixed-top');
  });

  $(window).scroll(function() {
  ($(document).scrollTop() > 500) ?
    $('#top').show():
    $('#top').hide();
  });

  $('#top').on('click', function() {
    $("html, body").animate({ scrollTop: 0 }, "fast");
  });

  // instead of jquery easing plugin
  $(function() {
  $('a[href*=#]:not([href=#])').click(function() {
    if (location.pathname.replace(/^\//,'') == this.pathname.replace(/^\//,'') && location.hostname == this.hostname) {
      var target = $(this.hash);
      target = target.length ?
        target :
        $('[name=' + this.hash.slice(1) +']');
      if (target.length) {
        $('html,body').animate({
          scrollTop: target.offset().top
        }, 'slow');
        return false;
        }
      }
    });
  });
});
