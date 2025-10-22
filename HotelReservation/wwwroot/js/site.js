$(document).ready(function () {
    const $slider = $('.brand-slider');
    if ($slider.length) {
        console.log("✅ Brand slider başlatılıyor...");
        $slider.slick({
            slidesToShow: 5,
            slidesToScroll: 1,
            autoplay: true,
            autoplaySpeed: 2000,
            arrows: false,
            dots: false,
            pauseOnHover: false,
            responsive: [
                { breakpoint: 992, settings: { slidesToShow: 3 } },
                { breakpoint: 576, settings: { slidesToShow: 2 } }
            ]
        });
    } else {
        console.log("⚠️ .brand-slider bulunamadı!");
    }
});
