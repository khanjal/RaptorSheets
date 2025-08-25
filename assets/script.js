// Smooth scrolling for navigation links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Copy to clipboard functionality
function copyToClipboard(button) {
    const codeBlock = button.previousElementSibling;
    const text = codeBlock.textContent;
    
    navigator.clipboard.writeText(text).then(function() {
        const originalIcon = button.innerHTML;
        button.innerHTML = '<i class="fas fa-check"></i>';
        button.style.background = '#10b981';
        
        setTimeout(function() {
            button.innerHTML = originalIcon;
            button.style.background = '';
        }, 2000);
    }).catch(function(err) {
        console.error('Failed to copy text: ', err);
    });
}

// Add loading animation for external links
document.querySelectorAll('a[target="_blank"]').forEach(link => {
    link.addEventListener('click', function() {
        // Add a subtle loading indicator if needed
        this.style.opacity = '0.7';
        setTimeout(() => {
            this.style.opacity = '';
        }, 500);
    });
});

// Intersection Observer for scroll animations
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver(function(entries) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.opacity = '1';
            entry.target.style.transform = 'translateY(0)';
        }
    });
}, observerOptions);

// Observe elements for scroll animation
document.addEventListener('DOMContentLoaded', function() {
    const animateElements = document.querySelectorAll('.feature, .package, .project, .doc');
    
    animateElements.forEach(element => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(20px)';
        element.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(element);
    });
});

// Header scroll behavior
let lastScrollTop = 0;
const header = document.querySelector('.header');

window.addEventListener('scroll', function() {
    let scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    
    if (scrollTop > lastScrollTop && scrollTop > 100) {
        // Scrolling down
        header.style.transform = 'translateY(-100%)';
    } else {
        // Scrolling up
        header.style.transform = 'translateY(0)';
    }
    
    lastScrollTop = scrollTop <= 0 ? 0 : scrollTop;
}, { passive: true });

// Mobile menu toggle (if needed in future)
function toggleMobileMenu() {
    const menu = document.querySelector('.nav__menu');
    menu.classList.toggle('nav__menu--active');
}

// Add subtle parallax effect to hero section
window.addEventListener('scroll', function() {
    const scrolled = window.pageYOffset;
    const hero = document.querySelector('.hero');
    if (hero) {
        hero.style.transform = `translateY(${scrolled * 0.5}px)`;
    }
}, { passive: true });

// Preload images for better performance
function preloadImages() {
    const imageUrls = [
        'https://github.com/khanjal/RaptorSheets/actions/workflows/dotnet.yml/badge.svg',
        'https://img.shields.io/nuget/v/RaptorSheets.Gig',
        'https://img.shields.io/nuget/v/RaptorSheets.Stock',
        'https://sonarcloud.io/api/project_badges/measure?project=khanjal_RaptorSheets&metric=coverage',
        'https://img.shields.io/github/license/khanjal/RaptorSheets'
    ];
    
    imageUrls.forEach(url => {
        const img = new Image();
        img.src = url;
    });
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    preloadImages();
    
    // Add loading animation to buttons
    document.querySelectorAll('.btn').forEach(button => {
        button.addEventListener('click', function(e) {
            if (!this.classList.contains('btn--disabled')) {
                this.style.transform = 'scale(0.98)';
                setTimeout(() => {
                    this.style.transform = '';
                }, 150);
            }
        });
    });
});

// Error handling for external resources
window.addEventListener('error', function(e) {
    if (e.target.tagName === 'IMG') {
        console.warn('Failed to load image:', e.target.src);
        // Could add fallback image handling here
    }
});

// Add keyboard navigation support
document.addEventListener('keydown', function(e) {
    // ESC key to scroll to top
    if (e.key === 'Escape') {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
});

// Performance optimization: Lazy load non-critical resources
function lazyLoadNonCritical() {
    // This could be expanded to lazy load images or other resources
    // For now, just ensuring Font Awesome icons load properly
    if (document.fonts) {
        document.fonts.ready.then(() => {
            console.log('Fonts loaded successfully');
        });
    }
}

// Initialize lazy loading
document.addEventListener('DOMContentLoaded', lazyLoadNonCritical);
