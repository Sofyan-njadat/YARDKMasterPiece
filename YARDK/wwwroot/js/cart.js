/**
 * Cart functionality for YARDK e-commerce platform
 */

// Function to update cart quantity
function updateQuantity(productId, change) {
    const quantityInput = document.getElementById(`qty-${productId}`);
    let newQuantity = parseInt(quantityInput.value) + change;
    
    if (newQuantity < 1) newQuantity = 1;
    
    quantityInput.value = newQuantity;
    
    // Update cart via AJAX
    fetch(`/Product/UpdateCartQuantity?id=${productId}&quantity=${newQuantity}`, {
        method: 'POST'
    })
    .then(response => {
        if (response.ok) {
            // Reload the page to reflect changes
            window.location.reload();
        }
    });
}

// Add to cart function for AJAX calls
function addToCart(productId) {
    fetch(`/Product/AddToCart?id=${productId}`, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Show success message
            showCartNotification(data.message);
            
            // Update cart count in navbar
            updateCartCount();
        } else {
            console.error('Failed to add product to cart');
        }
    })
    .catch(error => {
        console.error('Error:', error);
    });
}

// Function to show cart notification
function showCartNotification(message) {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = 'cart-notification';
    notification.innerHTML = `
        <div class="cart-notification-content">
            <i class="fas fa-check-circle"></i>
            <span>${message}</span>
        </div>
    `;
    
    // Add to document
    document.body.appendChild(notification);
    
    // Remove after 3 seconds
    setTimeout(() => {
        notification.classList.add('fade-out');
        setTimeout(() => {
            document.body.removeChild(notification);
        }, 500);
    }, 3000);
}

// Function to update cart count in navbar
function updateCartCount() {
    fetch('/Product/GetCartCount', {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        const cartCountElement = document.getElementById('cart-count');
        if (cartCountElement) {
            cartCountElement.textContent = data.count;
            cartCountElement.style.display = data.count > 0 ? 'inline-block' : 'none';
        }
    })
    .catch(error => {
        console.error('Error:', error);
    });
}

// Initialize cart functionality when page loads
document.addEventListener('DOMContentLoaded', function() {
    // Initialize any cart related elements
    updateCartCount();
    
    // Add event listeners for add to cart buttons
    const addToCartButtons = document.querySelectorAll('.add-to-cart-btn');
    addToCartButtons.forEach(button => {
        button.addEventListener('click', function(event) {
            event.preventDefault();
            const productId = this.getAttribute('data-product-id');
            addToCart(productId);
        });

    });
}); 
}); 