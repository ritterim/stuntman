$(document).ready(function($) {

  $(window).bind('scroll', function() {
    ($(window).scrollTop() > 50) ?
      $('#header').addClass('navbar-fixed-top'):
      $('#header').removeClass('navbar-fixed-top');
  });

  $('a.scrollto').on('click', function(e){
    e.preventDefault();

    var target = this.hash;

    $('body').scrollTo(target, 800, {offset: -70, 'axis':'y', easing:'easeOutQuad'});
    //Collapse mobile menu after clicking
    if ($('.navbar-collapse').hasClass('in')){
      $('.navbar-collapse').removeClass('in').addClass('collapse');
    }
  });

});
