// script.js

// Add event listeners to buttons for hover effect
document.querySelectorAll('.btn').forEach(button => {
    button.addEventListener('mouseover', () => {
        button.style.backgroundColor = '#0056b3'; // Darker blue color on hover
    });

    button.addEventListener('mouseout', () => {
        button.style.backgroundColor = '#007bff'; // Original blue color on mouseout
    });
});
