 
// ----- Form helper functions (kept global) -----

const mapProfil = {
    0: "SUPER_ADMIN",
    1: "ADMIN",
    2: "MON_OFFICER",
    3: "MON_MANAGER",
    4: "COMPTABLE"

};
function showToast(message, type) {
    try {
        const modal = document.getElementById("createProfileModal");
        if (modal) modal.style.display = "none";

        const toastBody = document.getElementById("toastBody");
        if (toastBody) toastBody.textContent = message || "";
        else console.warn("Élément #toastBody introuvable.");

        const toastHeader = document.getElementById("toastHeader");
        if (toastHeader) {
            toastHeader.classList.remove(
                "bg-success", "bg-warning", "bg-danger", "bg-info", "bg-secondary",
                "text-white", "text-dark"
            );

            switch ((type || "").toLowerCase()) {
                case "success":
                    toastHeader.classList.add("bg-success", "text-white");
                    break;
                case "warning":
                    toastHeader.classList.add("bg-warning", "text-dark");
                    break;
                case "error":
                case "danger":
                    toastHeader.classList.add("bg-danger", "text-white");
                    break;
                case "info":
                    toastHeader.classList.add("bg-info", "text-dark");
                    break;
                default:
                    toastHeader.classList.add("bg-secondary", "text-white");
                    break;
            }
        } else {
            console.warn("Élément #toastHeader introuvable.");
        }

        const toastElement = document.getElementById("liveToast");
        if (toastElement && window.bootstrap && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement);
            toast.show();
        } else {
            console.warn("Élément #liveToast introuvable ou Bootstrap non chargé.");
        }
    } catch (ex) {
        console.error('showToast error', ex);
    }
}

function toggleOnLoaderAndToast() {
    const loaderOverlay = document.getElementById("loaderOverlay");
    if (loaderOverlay) loaderOverlay.classList.add("show");
}

function toggleOffLoaderAndToast() {
    const loaderOverlay = document.getElementById("loaderOverlay");
    if (loaderOverlay) loaderOverlay.classList.remove("show");
}

function showLoader(message, title) {
    const loaderOverlay = document.getElementById("loaderOverlay");
    if (loaderOverlay) loaderOverlay.classList.add("show");

    setTimeout(() => {
        if (loaderOverlay) loaderOverlay.classList.remove("show");
        showToast(message, title);
    }, 100);
}

function LoginAsync(event) {
    event.preventDefault();
    toggleOnLoaderAndToast();

    let formData = new FormData(event.target);
    let data = Object.fromEntries(formData.entries());

    fetch("/Auth/LoginAsync", {
        method: "POST",
        credentials: 'same-origin',
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (!response.ok) {
                toggleOffLoaderAndToast();
                throw new Error("Erreur réseau");
            }
            return response.json();
        })
        .then(result => {
            if (result.success) {
                toggleOffLoaderAndToast();
                showToast(result.message, "success");
                setTimeout(() => { window.location.href = "/Profil/Index"; }, 1500);
            } else {
                toggleOffLoaderAndToast();
                showToast(result.message, "danger");
            }
        })
        .catch(error => {
            toggleOffLoaderAndToast();
            showToast("Erreur : " + error.message, "danger");
        });
}

async function DownloadBkmvti(event) {
    try {
        event.preventDefault();
        toggleOnLoaderAndToast();

        const typeMag = document.getElementById("typeMag").value;

        const response = await fetch("/ManqueAGagner/DownloadBkmvti", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ typeMag: typeMag })
        });

        if (!response.ok) {
            throw new Error("Erreur lors du téléchargement");
        }

        // IMPORTANT : lire en blob (fichier)
        const blob = await response.blob();

        // créer lien de téléchargement
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;

        // nom du fichier (optionnel)
        a.download = "BKMVTI.txt";

        document.body.appendChild(a);
        a.click();
        a.remove();

        toggleOffLoaderAndToast();
        showToast("Téléchargement réussi", "success");

    } catch (error) {
        toggleOffLoaderAndToast();
        showToast("Erreur : " + error.message, "danger");
    }
}

async function SaveMag(event) {
    try {
        event.preventDefault();
        toggleOnLoaderAndToast();

        let formData = new FormData(event.target);

        const response = await fetch("/ManqueAGagner/ProcessMagFiles", {
            method: "POST",
            body: formData  
        });

        if (!response.ok) {
            throw new Error("Erreur réseau");
        }

        const result = await response.json();

        if (result.success) {
            toggleOffLoaderAndToast();
            showToast(result.message, "success");
            setTimeout(() => { location.reload(); }, 1000);
        } else {
            toggleOffLoaderAndToast();
            showToast(result.message, "warning");
        }

    } catch (error) {
        toggleOffLoaderAndToast();
        showToast("Erreur : " + error.message, "danger");
    }
}
// Optionnel : intercepter le submit natif pour appeler SaveMag
//document.addEventListener('DOMContentLoaded', function () {
//    const form = document.getElementById('createMagModal');
//    form.addEventListener('submit', function (e) {
//        e.preventDefault();
//        SaveMag(e);
//    });
//});
function saveProfil(event) {
    event.preventDefault();
    toggleOnLoaderAndToast();

    let formData = new FormData(event.target);
    let data = Object.fromEntries(formData.entries());

    fetch("/Profil/saveProfil", {
        method: "POST",
        credentials: 'same-origin',
        headers: {
            "Content-Type": "application/json",
            "X-Requested-With": "XMLHttpRequest"
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (!response.ok) {
                toggleOffLoaderAndToast();
                throw new Error("Erreur réseau");
            }
            return response.json();
        })
        .then(result => {
            if (result.success) {
                toggleOffLoaderAndToast();
                showToast(result.message, "success");
                setTimeout(() => { location.reload(); }, 1000);
            } else {
                toggleOffLoaderAndToast();
                showToast(result.message, "warning");
            }
        })
        .catch(error => {
            toggleOffLoaderAndToast();
            showToast("Erreur : " + error.message, "danger");
        });
}

// ----- End form helpers -----

// DOM-ready modal logic
document.addEventListener('DOMContentLoaded', () => {
    // Récupération des éléments de la page
    const modal = document.getElementById("createProfileModal");
    const openBtn = document.getElementById("openProfileModalBtn");
    const closeBtn = document.getElementById("closeModalBtn");

    const magBtn = document.getElementById("openMagModalBtn")
    const openMagModal = document.getElementById("createMagModal")
    const saveMag = document.getElementById("submitMagBtn");

    // Sélectionner tous les boutons Cancel 
    const cancelButtons = document.querySelectorAll(".cancelBtn");

    // modal update
    const updateModal = document.getElementById("updateProfileModal");
    const openUpdateBtns = document.querySelectorAll(".openUpdateProfileModalBtn");
    //const closeUpdateModalBtn = document.getElementById("closeUpdateModalBtn");
    //const cancelUpdateModalBtn = document.getElementById("cancelUpdateBtn");

    // modal alert / suppression
    const modalAlert = document.getElementById("openAlertParameter");
    const deleteProfilBtn = document.getElementById("deleteProfilBtn");

    // Helper to hide all modals
    function hideAllModals() {
        if (modal) modal.style.display = "none";
        if (updateModal) updateModal.style.display = "none";
        if (modalAlert) modalAlert.style.display = "none";
        if (openMagModal) openMagModal.style.display = "none";
    }

    // Ouvrir le modal principal
    if (openBtn && modal) {
        openBtn.addEventListener('click', () => { modal.style.display = 'block'; });
    }

    if (magBtn && openMagModal) {
        magBtn.addEventListener('click', () => { openMagModal.style.display = 'block'; });
    }


    // Fermer avec les boutons cancel (tous)
    if (cancelButtons && cancelButtons.length) {
        cancelButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                hideAllModals();
            });
        });
    }

    // Fermer si clic à l'extérieur (handler unique)
    window.addEventListener('click', (event) => {
        const target = event.target;
        if (modal && target === modal) hideAllModals();
        if (updateModal && target === updateModal) hideAllModals();
        if (modalAlert && target === modalAlert) hideAllModals();
        if (openMagModal && target === openMagModal) hideAllModals(); 
    });



    // Suppression: écouteur sur le bouton delete (si présent)
    if (deleteProfilBtn) {
        deleteProfilBtn.addEventListener("click", function (event) {
            event.preventDefault();

            const id = this.getAttribute("data-id"); // Récupérer l'ID stocké

            fetch(`/Profil/DeleteProfil/${id}`, {
                method: "DELETE",
                credentials: 'same-origin',
                headers: {
                    "Content-Type": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                }
            })
                .then(response => {
                    if (!response.ok) throw new Error("Erreur lors de la suppression");
                    return response.json();
                })
                .then(result => {
                    if (result.success) {
                        // suppose showToast existe globalement
                        showToast(result.message, "success");
                        if (modalAlert) modalAlert.style.display = "none";
                        setTimeout(() => { location.reload(); }, 1000);
                    } else {
                        showToast(result.message, "warning");
                    }
                })
                .catch(error => {
                    showToast("Erreur : " + error.message, "danger");
                });
        });
    }

    // Attacher un événement à chaque bouton "Modifier" pour ouvrir updateModal
    if (openUpdateBtns && openUpdateBtns.length) {
        openUpdateBtns.forEach(btn => {
            btn.addEventListener("click", function () {
                const id = this.getAttribute("data-id");

                // Charger les données depuis le serveur
                fetch(`/Profil/GetProfil/${id}`, {
                    credentials: 'same-origin',
                    headers: {
                        "X-Requested-With": "XMLHttpRequest",
                        "Content-Type": "application/json"
                    }
                })
                    .then(response => {
                        if (response.status === 440 || response.status === 401) {
                            window.location.href = "/Auth/Login";
                            return;
                        }
                        if (!response.ok) {
                            throw new Error('Erreur réseau');
                        }
                        return response.json();
                    })
                    .then(result => {
                        if (!result) return; // cas où redirect a eu lieu
                        if (!result.success) {
                            alert(result.message);
                            return;
                        }

                        const profil = result.data || {};
  

                        // Remplir le formulaire si les éléments existent
                        const setIfExists = (id, value) => {
                            const el = document.getElementById(id);
                            if (el) el.value = value ?? '';
                        }; 
                        setIfExists("Username", profil.username);
                        setIfExists("Userag", profil.userag);
                        

                        setIfExists("TypeProfileString", mapProfil[profil.typeProfile] || "Inconnu");
                        setIfExists("Status", profil.status);
                        setIfExists("Email", profil.email);

                        // Afficher le modal de mise à jour
                        if (updateModal) updateModal.style.display = "block";
                    })
                    .catch(err => alert( "Error : " +err.message));
            });
        });
    }
});

// Fonction appelée depuis le bouton du tableau pour le modal de confirmation de suppression
function showDeleteProfile(id) {
    const modalAlert = document.getElementById("openAlertParameter");
    const deleteProfilBtn = document.getElementById("deleteProfilBtn");
    if (!modalAlert || !deleteProfilBtn) return;

    // Stocker l'ID dans un attribut data-id
    deleteProfilBtn.setAttribute("data-id", id);

    // Afficher le modal
    modalAlert.style.display = "block";
}

// Expose function globally (necessary if called from markup)
window.showDeleteProfile = showDeleteProfile;

