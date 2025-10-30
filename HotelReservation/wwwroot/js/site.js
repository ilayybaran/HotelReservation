$(document).ready(function(){

    const $brandSlider = $('.continuous-brand-slider'); // Target the new class

    if ($brandSlider.length) {
        console.log("✅ Continuous Brand slider başlatılıyor...");
        $brandSlider.slick({
            slidesToShow: 4,      // Adjust based on logo size and desired density
            slidesToScroll: 1,
            autoplay: true,
            autoplaySpeed: 0,       // No delay between animations
            speed: 5000,      // DURATION of the scroll animation (higher = slower) - Adjust this!
            cssEase: 'linear',    // Constant speed, no easing
            infinite: true,
            arrows: false,
            dots: false,
            pauseOnHover: false,    // Keep scrolling on hover
            responsive: [
                {
                    breakpoint: 1200,
                    settings: { slidesToShow: 5 }
                },
                {
                    breakpoint: 992,
                    settings: { slidesToShow: 4 }
                },
                {
                    breakpoint: 768,
                    settings: { slidesToShow: 3 }
                },
                {
                    breakpoint: 576,
                    settings: { slidesToShow: 2 }
                }
            ]
        });
    } else {
        console.log("⚠️ .continuous-brand-slider bulunamadı!");
    }

    // --- Keep your other Slick initializations if you have them ---
    // Example: $('.other-slider').slick({ ... });

}); // document ready end



document.addEventListener('DOMContentLoaded', function () {
    
    flatpickr.localize(flatpickr.l10ns.tr);

    flatpickr(".flatpickr-input", {
        altInput: true,
        altFormat: "d-m-Y",
        dateFormat: "Y-m-d",
        allowInput: true,
        minDate: "today",       
        disableMobile: "true" ,
        enableTime: false,   
    });
});

