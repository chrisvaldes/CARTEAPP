
"use strict"

// for show password 
let createpassword = (type, ele) => {
    document.getElementById(type).type = document.getElementById(type).type == "password" ? "text" : "password"
    let icon = ele.childNodes[0].classList
    let stringIcon = icon.toString()
    if (stringIcon.includes("fa-solid fa-eye")) {
        ele.childNodes[0].classList.remove("fa-solid fa-eye")
        ele.childNodes[0].classList.add("fa-solid fa-eye-slash")
    }
    else {
        ele.childNodes[0].classList.add("fa-solid fa-eye")
        ele.childNodes[0].classList.remove("fa-solid fa-eye-slash")
    }
}