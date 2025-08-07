// A simple TypeScript modal dialog implementation without React
// Usage: import { showConfirmDialog } from './ConfirmDialog';
// await showConfirmDialog('Are you sure?')

export function showConfirmDialog(message: string): Promise<boolean> {
  return new Promise((resolve) => {
    // Create modal elements
    const overlay = document.createElement('div');
    overlay.style.position = 'fixed';
    overlay.style.top = '0';
    overlay.style.left = '0';
    overlay.style.width = '100vw';
    overlay.style.height = '100vh';
    overlay.style.background = 'rgba(0,0,0,0.4)';
    overlay.style.display = 'flex';
    overlay.style.alignItems = 'center';
    overlay.style.justifyContent = 'center';
    overlay.style.zIndex = '9999';

    const dialog = document.createElement('div');
    dialog.style.background = '#fff';
    dialog.style.padding = '2rem';
    dialog.style.borderRadius = '8px';
    dialog.style.boxShadow = '0 2px 16px rgba(0,0,0,0.2)';
    dialog.style.minWidth = '300px';
    dialog.style.textAlign = 'center';

    const msg = document.createElement('div');
    msg.textContent = message;
    msg.style.marginBottom = '1.5rem';
    dialog.appendChild(msg);

    const btnYes = document.createElement('button');
    btnYes.textContent = 'Yes';
    btnYes.style.marginRight = '1rem';
    btnYes.style.padding = '0.5rem 1.5rem';
    btnYes.style.background = '#d9534f';
    btnYes.style.color = '#fff';
    btnYes.style.border = 'none';
    btnYes.style.borderRadius = '4px';
    btnYes.style.cursor = 'pointer';
    btnYes.onclick = () => {
      document.body.removeChild(overlay);
      resolve(true);
    };

    const btnNo = document.createElement('button');
    btnNo.textContent = 'No';
    btnNo.style.padding = '0.5rem 1.5rem';
    btnNo.style.background = '#6c757d';
    btnNo.style.color = '#fff';
    btnNo.style.border = 'none';
    btnNo.style.borderRadius = '4px';
    btnNo.style.cursor = 'pointer';
    btnNo.onclick = () => {
      document.body.removeChild(overlay);
      resolve(false);
    };

    dialog.appendChild(btnYes);
    dialog.appendChild(btnNo);
    overlay.appendChild(dialog);
    document.body.appendChild(overlay);
  });
}
