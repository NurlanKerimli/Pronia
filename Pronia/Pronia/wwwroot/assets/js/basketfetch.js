const addButtons = document.querySelectorAll("button");

addButtons.forEach(btn => {
    btn.addEventListener("click", function (e) {
        e.preventDefault();
        var endpoint = btn.getAttribute("href");

        fetch(endpoint)
            .then(response => response.text())
            .then(data => {
                console.log(data)
            })
        
    })
})