function setupUploader(fileInputId, hiddenInputId, previewImageId = null, uploadUrl = '/Admin/UploadImage') {
    const fileInput = document.getElementById(fileInputId);
    const hiddenInput = document.getElementById(hiddenInputId);

    if (!fileInput || !hiddenInput) {
        console.error('Uploader setup error: Input elements not found.');
        return;
    }

    fileInput.addEventListener('change', async function (event) {
        const file = event.target.files[0];
        if (!file) return;

        const formData = new FormData();
        formData.append('image', file);

        try {
            const response = await fetch(uploadUrl, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error('Upload failed');
            }

            const data = await response.json();

            if (data.location) {
                hiddenInput.value = data.location;

                if (previewImageId) {
                    const preview = document.getElementById(previewImageId);
                    if (preview) {
                        preview.src = data.location;
                        preview.style.display = 'block';
                    }
                }
            }
        } catch (error) {
            console.error('Upload error:', error);
            alert('Image upload failed.');
        }
    });

}
