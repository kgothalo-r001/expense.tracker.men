// Add health check tab to Swagger UI
(function() {
    'use strict';
    
    // Wait for Swagger UI to load
    setTimeout(function() {
        addHealthCheckTab();
    }, 1000);
    
    function addHealthCheckTab() {
        const topbar = document.querySelector('.topbar');
        if (!topbar) {
            setTimeout(addHealthCheckTab, 500);
            return;
        }
        
        // Create health check button
        const healthButton = document.createElement('button');
        healthButton.className = 'btn health-check-btn';
        healthButton.textContent = 'Health Check';
        healthButton.style.cssText = `
            background-color: #49cc90;
            color: white;
            border: none;
            padding: 8px 16px;
            margin-left: 10px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
        `;
        
        // Add click handler
        healthButton.addEventListener('click', function() {
            showHealthCheck();
        });
        
        // Insert button into topbar
        const topbarWrapper = topbar.querySelector('.topbar-wrapper');
        if (topbarWrapper) {
            topbarWrapper.appendChild(healthButton);
        }
    }
    
    function showHealthCheck() {
        // Create modal overlay
        const overlay = document.createElement('div');
        overlay.className = 'health-check-overlay';
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
        `;
        
        // Create modal content
        const modal = document.createElement('div');
        modal.className = 'health-check-modal';
        modal.style.cssText = `
            background: white;
            border-radius: 8px;
            padding: 20px;
            max-width: 600px;
            width: 90%;
            max-height: 80%;
            overflow-y: auto;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
        `;
        
        modal.innerHTML = `
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
                <h2 style="margin: 0; color: #3b4151;">Health Check Status</h2>
                <button class="close-btn" style="background: none; border: none; font-size: 24px; cursor: pointer; color: #999;">&times;</button>
            </div>
            <div id="health-status" style="text-align: center; padding: 20px;">
                <div style="font-size: 18px; color: #666;">Loading health status...</div>
            </div>
        `;
        
        overlay.appendChild(modal);
        document.body.appendChild(overlay);
        
        // Close modal handlers
        overlay.addEventListener('click', function(e) {
            if (e.target === overlay) {
                document.body.removeChild(overlay);
            }
        });
        
        modal.querySelector('.close-btn').addEventListener('click', function() {
            document.body.removeChild(overlay);
        });
        
        // Fetch health status
        fetchHealthStatus(modal.querySelector('#health-status'));
    }
    
    function fetchHealthStatus(container) {
        fetch('/health')
            .then(response => response.json())
            .then(data => {
                const statusColor = data.status === 'Healthy' ? '#49cc90' : '#f93e3e';
                
                container.innerHTML = `
                    <div style="margin-bottom: 20px;">
                        <div style="font-size: 24px; font-weight: bold; color: ${statusColor}; margin-bottom: 10px;">
                            ${data.status}
                        </div>
                        <div style="font-size: 14px; color: #666;">
                            Last checked: ${new Date(data.timestamp).toLocaleString()}
                        </div>
                        <div style="font-size: 14px; color: #666;">
                            Total duration: ${data.totalDuration}ms
                        </div>
                    </div>
                    <div style="text-align: left;">
                        <h3 style="color: #3b4151; margin-bottom: 15px;">Health Checks:</h3>
                        ${data.checks.map(check => `
                            <div style="padding: 10px; margin-bottom: 10px; border-left: 4px solid ${check.status === 'Healthy' ? '#49cc90' : '#f93e3e'}; background-color: #f7f7f7;">
                                <div style="font-weight: bold; color: #3b4151;">${check.name}</div>
                                <div style="font-size: 14px; color: #666;">Status: ${check.status}</div>
                                <div style="font-size: 14px; color: #666;">Duration: ${check.duration}ms</div>
                                ${check.description ? `<div style="font-size: 14px; color: #666;">${check.description}</div>` : ''}
                            </div>
                        `).join('')}
                    </div>
                `;
            })
            .catch(error => {
                container.innerHTML = `
                    <div style="color: #f93e3e; font-size: 18px; margin-bottom: 10px;">
                        Error loading health status
                    </div>
                    <div style="font-size: 14px; color: #666;">
                        ${error.message}
                    </div>
                `;
            });
    }
})();
