/**
   * Fetches invoice details by ID and populates the modal.
   * This is an asynchronous operation using the Fetch API (AJAX).
   */
  /*Client - side AJAX logic using Fetch API */
function showInvoiceModal(invoiceId) {
    fetch(`/Invoice/GetInvoiceJson/${invoiceId}`)
        .then(res => res.json()) // Parse JSON response
        .then(data => {
            // Populate modal with invoice data
            document.getElementById("invNumber").innerText = data.invoiceNumber;
            document.getElementById("customerName").innerText = data.customerName;
            document.getElementById("vendorName").innerText = data.vendorName;
            document.getElementById("invoiceDate").innerText = new Date(data.invoiceDate).toLocaleDateString();
            document.getElementById("dueDate").innerText = new Date(data.dueDate).toLocaleDateString();
            document.getElementById("amountPaid").innerText = parseFloat(data.amountPaid).toFixed(2);

            // Generate table rows for items
            let rows = '';
            data.items.forEach(i => {
                const total = (i.unitPrice * i.quantity).toFixed(2);
                rows += `
                        <tr>
                            <td>${i.description}</td>
                            <td>₹${i.unitPrice.toFixed(2)}</td>
                            <td>${i.quantity}</td>
                            <td>₹${total}</td>
                        </tr>
                    `;
            });

            // Insert rows into modal table
            document.getElementById("itemTableBodyModal").innerHTML = rows;

            // Show the modal using Bootstrap
            const modal = new bootstrap.Modal(document.getElementById('invoiceModal'));
            modal.show();
        })
        .catch(err => {
            // Handle fetch errors
            alert("Failed to load invoice details.");
            console.error(err);
        });
}

