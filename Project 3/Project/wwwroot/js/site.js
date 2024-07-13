// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//quantity change
var proQty = $('.pro-qty');
proQty.prepend('<span class="dec qtybtn">-</span>');
proQty.append('<span class="inc qtybtn">+</span>');
proQty.on('click', '.qtybtn', function () {
    var $button = $(this);
    var $input = $button.parent().find('input');
    var oldValue = parseFloat($input.val());
    var step = parseFloat($input.attr('step')) || 1;

    if ($button.hasClass('inc')) {
        var newVal = oldValue + step;
    } else {
        if (oldValue > step) {
            var newVal = oldValue - step;
        } else {
            newVal = 0;
        }
    }

    $input.val(newVal);

    updateCartQuantity($input);
});

$('.quantity-input').on('input', function () {
    updateCartQuantity(this);
});

function updateCartQuantity(inputElement) {
    var bookId = $(inputElement).data('pid');
    var newQuantity = $(inputElement).val();

    $.ajax({
        url: '/Cart/UpdateQuantity/' + bookId,
        type: 'POST',
        data: { newQuantity: newQuantity },
        success: function (data) {
            location.reload();
        },
        error: function () {
            console.log('Error updating cart quantity.');
        }
    });
}

