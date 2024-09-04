var images = [
    "/img/imagem1.jpg",
    "/img/imagem2.jpg",
    "/img/imagem3.jpg"
];

var currentIndex = 0;

function validateEmailAndPassword() {
    var inputEmail = document.getElementById("Email").value;
    var inputSenha = document.getElementById("Senha").value;

    if (inputEmail.trim() === "" || inputSenha.trim() === "") {
        alert("Por favor, preencha todos os campos.");
        return false;
    }

    return true;
}

function changeImage() {
    var imageElement = document.getElementById("myImage");
    currentIndex++;
    if (currentIndex >= images.length) {
        currentIndex = 0;
    }
    imageElement.src = images[currentIndex];
}

function validateMec() {
    var inputMec = parseInt(document.getElementById("Mec").value);
    if (isNaN(inputMec) || inputMec < 1) {
        alert("Número Mec inválido!");
        return false;
    }
    return validateName();
}
function validateName() {
    var inputName = document.getElementById("Nome").value;
    var regex = /^[a-zA-Z\s]+$/; // Only allow alphabetic characters and spaces

    if (!regex.test(inputName)) {
        alert("Por favor, introduza o nome corretamente.");
        return false;
    }

    return validateSurname();
}

function validateSurname() {
    var inputSurname = document.getElementById("Apelido").value;
    var regex = /^[a-zA-Z\s]+$/; // Only allow alphabetic characters and spaces

    if (!regex.test(inputSurname)) {
        alert("Por favor, introduza o nome corretamente.");
        return false;
    }

    return validateAge();
}
function validateAge() {
    var age = parseInt(document.getElementById("Idade").value);
    if (isNaN(age) || age < 18) {
        alert("Você deve ter 18 anos ou mais para se inscrever na escola militar.");
        return false;
    }
    return validateElse();
}
function validateElse() {
    return true;
}

