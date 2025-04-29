// Opens the Bootstrap Modal and loads the Upload Manager
function openImageSelector(targetInputId, folder) {
    const iframe = document.getElementById('uploadManagerIframe');
    iframe.src = `/Admin/BrowseImages?folder=${folder}&target=${targetInputId}`;
    const modal = new bootstrap.Modal(document.getElementById('uploadManagerModal'));
    modal.show();
}

// Called by the modal iframe to set the selected image
function selectImage(imagePath, targetInputId) {
    const targetInput = document.getElementById(targetInputId);
    if (targetInput) {
        targetInput.value = imagePath;

        // Trigger change event in case listeners are attached
        targetInput.dispatchEvent(new Event('change'));

        // Update preview image if one exists with a matching ID
        const previewImage = document.getElementById(`${targetInputId}Preview`);
        if (previewImage) {
            previewImage.src = imagePath;
            previewImage.style.display = 'block';
        }

        // Update selected filename display
        const friendlyMap = {
            'PosterPath': 'posterSelectedFilename',
            'IconUrl': 'iconSelectedFilename',
            'BackgroundImageUrl': 'backgroundSelectedFilename'
        };

        const filenameDisplayId = friendlyMap[targetInputId];
        if (filenameDisplayId) {
            const filenameLabel = document.getElementById(filenameDisplayId);
            if (filenameLabel) {
                const filename = imagePath.split('/').pop();
                filenameLabel.innerHTML = `<strong>Selected:</strong> ${filename}`;
                filenameLabel.style.display = 'block';
            }
        }

        // Close the modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('uploadManagerModal'));
        if (modal) {
            modal.hide();
        }
    } else {
        console.error(`No input element found with id: ${targetInputId}`);
    }
}