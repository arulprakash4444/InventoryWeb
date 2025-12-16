console.log("Hi welcome");


const updateStockModal = document.getElementById('updateStockModal')

let currentStock;
    let updateStockbutton;

if (updateStockModal) {



    updateStockModal.addEventListener('show.bs.modal', event => {


        updateStockbutton = event.relatedTarget


        const productName = updateStockbutton.getAttribute('data-bs-productName');
        const currentStockEl = updateStockbutton.getAttribute('data-bs-currentStock');
        currentStock = Number(currentStockEl);

        const modalTitle = updateStockModal.querySelector('.modal-title');
        const modalBodyInput = updateStockModal.querySelector('.stock');

        modalTitle.textContent = `UPDATE STOCK: ${productName}`;
        modalBodyInput.textContent = `Current stock: ${currentStockEl}`;
    })

}

    let selectedAction = null;
    function selectAction(action) {
        selectedAction = action;

        const addBtn = document.getElementById("addBtn");
        const removeBtn = document.getElementById("removeBtn");
        const qtyInput = document.getElementById("qty");
        const okBtn = document.getElementById("okBtn");

        // Reset button styles
        addBtn.className = "btn btn-outline-success w-50";
        removeBtn.className = "btn btn-outline-danger w-50";

        // Turn selected button into filled style
        if (action === "add") {
            addBtn.className = "btn btn-success w-50";
            qtyInput.removeAttribute("max");
        } else {
            removeBtn.className = "btn btn-danger w-50";
            qtyInput.setAttribute("max", currentStock);
        }

        // Enable quantity input & reset value
        qtyInput.disabled = false;
        qtyInput.value = "";

        // Disable OK button until qty typed
        okBtn.disabled = true;
    }

    function checkQty() {
        const qtyInput = document.getElementById("qty");
        const okBtn = document.getElementById("okBtn");
        const qty = Number(qtyInput.value);

        // If no action chosen → disable OK
        if (!selectedAction) {
            okBtn.disabled = true;
            return;
        }

        // ADD STOCK (only condition: qty > 0)
        if (selectedAction === "add") {
            okBtn.disabled = !(qty > 0);
            return;
        }

        // REMOVE STOCK (qty must be between 1 and currentStock)
        if (selectedAction === "remove") {
            const max = Number(qtyInput.getAttribute("max")); // already set in selectAction()

            if (qty > 0 && qty <= max) {
                okBtn.disabled = false;
            } else {
                okBtn.disabled = true;
            }
        }
    }

    async function updateStock() {

        const qty = Number(document.getElementById("qty").value);

        const payload = {
            productId: Number(updateStockbutton.getAttribute("data-bs-productId")),
            action: selectedAction.toUpperCase(),   // "ADD" or "REMOVE"
            quantity: qty
        };



        try {
            const response = await fetch("/product/updatestock/", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                throw new Error(`Server error: ${response.status}`);
            }

            const data = await response.json();
            console.log("Server response:", data);

            //Close modal on success
            const modal = bootstrap.Modal.getInstance(updateStockModal);
            modal.hide();

            // Optional: update UI or reload
            location.reload();

        } catch (err) {
            console.error("Update stock failed:", err);
            // Optional: show error in UI
        }
    }


    // Reset the Notification, if left without OK'ing
    if (updateStockModal) {
        updateStockModal.addEventListener("hide.bs.modal", () => {

            selectedAction = null;

            const addBtn = document.getElementById("addBtn");
            const removeBtn = document.getElementById("removeBtn");
            const qtyInput = document.getElementById("qty");
            const okBtn = document.getElementById("okBtn");

            // Reset buttons to outline styles
            addBtn.className = "btn btn-outline-success w-50";
            removeBtn.className = "btn btn-outline-danger w-50";

            // Reset quantity input
            qtyInput.value = "";
            qtyInput.disabled = true;
            qtyInput.removeAttribute("max");

            // Disable OK button
            okBtn.disabled = true;

        });
    }





document.addEventListener("DOMContentLoaded", () => loadNotificationCount());

async function loadNotificationCount() {
    try {
        const response = await fetch('/notification/getunreadcount');

        if (!response.ok) {
            console.error("Failed to load notification count.");
            return;
        }

        const data = await response.json();
        const count = data.unreadCount;

        const wrapper = document.getElementById("notificationCount");
        if (!wrapper) return;

        // No unread notifications
        if (count === 0) {
            wrapper.innerHTML = `<i class="bi bi-bell"></i>`;
            return;
        }

        // Unread notifications > 0
        wrapper.innerHTML = `
                <i class="bi bi-bell-fill text-danger position-relative">
                    <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
                          style="font-size:12px;">
                        ${count}
                    </span>
                </i>`;
    }
    catch (error) {
        console.error("Error fetching notification count:", error);
    }
}

