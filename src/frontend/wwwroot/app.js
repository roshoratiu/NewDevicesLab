const state = {
    currentUser: null,
    overview: null,
    dashboard: null,
    selectedProjectId: null
};

const elements = {
    loginView: document.getElementById("loginView"),
    appView: document.getElementById("appView"),
    overlay: document.getElementById("overlay"),
    loginForm: document.getElementById("loginForm"),
    loginMessage: document.getElementById("loginMessage"),
    currentUserName: document.getElementById("currentUserName"),
    currentUserMeta: document.getElementById("currentUserMeta"),
    accountInitials: document.getElementById("accountInitials"),
    accountMenuButton: document.getElementById("accountMenuButton"),
    accountMenuPanel: document.getElementById("accountMenuPanel"),
    profileMenuButton: document.getElementById("profileMenuButton"),
    accountAdminButton: document.getElementById("accountAdminButton"),
    logoutButton: document.getElementById("logoutButton"),
    homeTitle: document.getElementById("homeTitle"),
    homeCopy: document.getElementById("homeCopy"),
    groupCount: document.getElementById("groupCount"),
    permissionCount: document.getElementById("permissionCount"),
    adminHeroCard: document.getElementById("adminHeroCard"),
    projectAccessHint: document.getElementById("projectAccessHint"),
    userGroups: document.getElementById("userGroups"),
    userPermissions: document.getElementById("userPermissions"),
    profilePanel: document.getElementById("profilePanel"),
    closeProfileButton: document.getElementById("closeProfileButton"),
    profileForm: document.getElementById("profileForm"),
    profileEmail: document.getElementById("profileEmail"),
    profileUsername: document.getElementById("profileUsername"),
    profileMessage: document.getElementById("profileMessage"),
    passwordForm: document.getElementById("passwordForm"),
    passwordMessage: document.getElementById("passwordMessage"),
    projectModal: document.getElementById("projectModal"),
    openProjectModalButton: document.getElementById("openProjectModalButton"),
    openProjectModalButtonAlt: document.getElementById("openProjectModalButtonAlt"),
    closeProjectModalButton: document.getElementById("closeProjectModalButton"),
    cancelProjectModalButton: document.getElementById("cancelProjectModalButton"),
    projectPreviewForm: document.getElementById("projectPreviewForm"),
    projectMessage: document.getElementById("projectMessage"),
    projectsList: document.getElementById("projectsList"),
    selectedProjectName: document.getElementById("selectedProjectName"),
    selectedProjectStatus: document.getElementById("selectedProjectStatus"),
    selectedProjectIdea: document.getElementById("selectedProjectIdea"),
    selectedProjectDescription: document.getElementById("selectedProjectDescription"),
    selectedProjectMeta: document.getElementById("selectedProjectMeta"),
    selectedProjectMembers: document.getElementById("selectedProjectMembers"),
    statusEditorSection: document.getElementById("statusEditorSection"),
    projectStatusSelect: document.getElementById("projectStatusSelect"),
    saveProjectStatusButton: document.getElementById("saveProjectStatusButton"),
    projectStatusMessage: document.getElementById("projectStatusMessage"),
    selectedProjectOrderSheets: document.getElementById("selectedProjectOrderSheets"),
    orderSheetSection: document.getElementById("orderSheetSection"),
    orderSheetForm: document.getElementById("orderSheetForm"),
    orderSheetItems: document.getElementById("orderSheetItems"),
    addOrderItemButton: document.getElementById("addOrderItemButton"),
    orderSheetMessage: document.getElementById("orderSheetMessage"),
    adminSection: document.getElementById("adminSection"),
    usersStat: document.getElementById("usersStat"),
    groupsStat: document.getElementById("groupsStat"),
    permissionsStat: document.getElementById("permissionsStat"),
    usersList: document.getElementById("usersList"),
    groupsList: document.getElementById("groupsList"),
    permissionsList: document.getElementById("permissionsList"),
    createUserForm: document.getElementById("createUserForm"),
    createUserMessage: document.getElementById("createUserMessage"),
    createGroupOptions: document.getElementById("createGroupOptions"),
    membershipForm: document.getElementById("membershipForm"),
    membershipUserSelect: document.getElementById("membershipUserSelect"),
    membershipGroupOptions: document.getElementById("membershipGroupOptions"),
    membershipMessage: document.getElementById("membershipMessage"),
    groupPermissionForm: document.getElementById("groupPermissionForm"),
    groupPermissionSelect: document.getElementById("groupPermissionSelect"),
    permissionOptions: document.getElementById("permissionOptions"),
    groupPermissionMessage: document.getElementById("groupPermissionMessage"),
    navLinks: [...document.querySelectorAll("[data-section-target]")]
};

async function apiRequest(url, options = {}) {
    const headers = { ...(options.headers || {}) };

    if (options.body && !headers["Content-Type"]) {
        headers["Content-Type"] = "application/json";
    }

    const response = await fetch(url, {
        credentials: "same-origin",
        ...options,
        headers
    });

    if (response.status === 204) {
        return null;
    }

    const contentType = response.headers.get("content-type") || "";
    const payload = contentType.includes("application/json")
        ? await response.json()
        : await response.text();

    if (!response.ok) {
        const message = typeof payload === "string" ? payload : payload?.title || "Request failed.";
        const error = new Error(message);
        error.status = response.status;
        throw error;
    }

    return payload;
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

function selectedValues(selector) {
    return [...document.querySelectorAll(selector)]
        .filter((item) => item.checked)
        .map((item) => item.value);
}

function hasPermission(permission) {
    return state.currentUser?.permissions?.includes("system.full_access")
        || state.currentUser?.permissions?.includes(permission);
}

function getInitials(name) {
    return name
        .split(/\s+/)
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part[0]?.toUpperCase() || "")
        .join("") || "U";
}

function getSelectedProject() {
    return state.dashboard?.projects?.find((project) => project.id === state.selectedProjectId) || null;
}

function closeAccountMenu() {
    elements.accountMenuPanel.classList.add("hidden");
}

function openOverlay() {
    elements.overlay.classList.remove("hidden");
}

function closeOverlay() {
    const hasOpenLayer = !elements.profilePanel.classList.contains("hidden")
        || !elements.projectModal.classList.contains("hidden");

    if (!hasOpenLayer) {
        elements.overlay.classList.add("hidden");
    }
}

function openProfilePanel() {
    closeAccountMenu();
    openOverlay();
    elements.profilePanel.classList.remove("hidden");
}

function closeProfilePanel() {
    elements.profilePanel.classList.add("hidden");
    closeOverlay();
}

function openProjectModal() {
    closeAccountMenu();
    openOverlay();
    elements.projectModal.classList.remove("hidden");
}

function closeProjectModal() {
    elements.projectModal.classList.add("hidden");
    closeOverlay();
    elements.projectMessage.textContent = "";
}

function showLogin() {
    closeAccountMenu();
    closeProfilePanel();
    closeProjectModal();
    elements.loginView.classList.remove("hidden");
    elements.appView.classList.add("hidden");
}

function showApp() {
    elements.loginView.classList.add("hidden");
    elements.appView.classList.remove("hidden");
}

function activateSection(sectionId) {
    elements.navLinks.forEach((button) => {
        button.classList.toggle("is-active", button.dataset.sectionTarget === sectionId);
    });

    document.getElementById(sectionId)?.scrollIntoView({ behavior: "smooth", block: "start" });
}

function createEmptyOrderItemRow() {
    const row = document.createElement("div");
    row.className = "order-item-grid";
    row.innerHTML = `
        <input type="text" name="siteName" placeholder="Site name" required />
        <input type="text" name="componentName" placeholder="Component name" required />
        <input type="text" name="brand" placeholder="Brand" required />
        <input type="url" name="link" placeholder="Link" required />
        <input type="number" name="priceEuro" min="0.01" step="0.01" placeholder="Price in euro" required />
        <button type="button" class="secondary-button" data-remove-order-item>Remove</button>
    `;

    return row;
}

function resetOrderSheetForm() {
    elements.orderSheetForm.reset();
    const rows = [...elements.orderSheetItems.querySelectorAll(".order-item-grid")];

    rows.forEach((row, index) => {
        if (index === 0) {
            row.querySelectorAll("input").forEach((input) => {
                input.value = "";
            });

            const removeButton = row.querySelector("[data-remove-order-item]");
            if (removeButton) {
                removeButton.remove();
            }
        } else {
            row.remove();
        }
    });
}

function renderUserShell() {
    const user = state.currentUser;
    const adminAccess = hasPermission("admin.access");

    elements.currentUserName.textContent = user.username;
    elements.currentUserMeta.textContent = user.radboudEmail;
    elements.accountInitials.textContent = getInitials(user.username);
    elements.groupCount.textContent = user.groups.length;
    elements.permissionCount.textContent = user.permissions.length;
    elements.profileEmail.value = user.radboudEmail;
    elements.profileUsername.value = user.username;

    elements.userGroups.innerHTML = user.groups.length
        ? user.groups.map((group) => `<span class="chip">${escapeHtml(group)}</span>`).join("")
        : '<span class="chip">No groups</span>';

    elements.userPermissions.innerHTML = user.permissions.length
        ? user.permissions.map((permission) => `<span class="chip">${escapeHtml(permission)}</span>`).join("")
        : '<span class="chip">No permissions</span>';

    elements.homeTitle.textContent = "Projects";

    if (adminAccess) {
        elements.homeCopy.textContent = "Your workspace is ready. Admin tools are available from your account menu.";
        elements.adminHeroCard.classList.remove("hidden");
        elements.accountAdminButton.classList.remove("hidden");
        elements.adminSection.classList.remove("hidden");
    } else {
        elements.homeCopy.textContent = "Your workspace is ready. Project and order workflows follow your assigned permissions.";
        elements.adminHeroCard.classList.add("hidden");
        elements.accountAdminButton.classList.add("hidden");
        elements.adminSection.classList.add("hidden");
    }
}

function renderProjectList() {
    const projects = state.dashboard?.projects ?? [];

    if (!projects.length) {
        elements.projectsList.innerHTML = '<p class="muted">No projects visible yet. Create one to get started.</p>';
        return;
    }

    elements.projectsList.innerHTML = projects.map((project) => `
        <article class="stack-item">
            <div class="project-card-head">
                <h4>${escapeHtml(project.name)}</h4>
                <span class="status-pill">${escapeHtml(project.status)}</span>
            </div>
            <p>${escapeHtml(project.idea)}</p>
            <p>${escapeHtml(project.createdByUsername)} · ${new Date(project.createdAtUtc).toLocaleDateString()}</p>
            <p>${project.orderSheets.length} order sheet(s)</p>
            <button type="button" class="secondary-button" data-project-id="${project.id}">
                ${project.id === state.selectedProjectId ? "Selected" : "Open"}
            </button>
        </article>
    `).join("");
}

function renderOrderSheets(project) {
    if (!project) {
        elements.selectedProjectOrderSheets.innerHTML = '<p class="muted">No project selected yet.</p>';
        return;
    }

    if (!project.orderSheets.length) {
        elements.selectedProjectOrderSheets.innerHTML = '<p class="muted">No order sheets yet for this project.</p>';
        return;
    }

    elements.selectedProjectOrderSheets.innerHTML = project.orderSheets.map((orderSheet) => `
        <article class="stack-item">
            <div class="project-card-head">
                <h4>${escapeHtml(orderSheet.createdByUsername)}</h4>
                <span class="status-pill">${escapeHtml(orderSheet.status)}</span>
            </div>
            <p>Total: EUR ${Number(orderSheet.totalEuro).toFixed(2)}</p>
            <p>${new Date(orderSheet.createdAtUtc).toLocaleString()}</p>
            <div class="stack-list">
                ${orderSheet.items.map((item) => `
                    <div>
                        <strong>${escapeHtml(item.componentName)}</strong>
                        <p>${escapeHtml(item.brand)} · ${escapeHtml(item.siteName)} · EUR ${Number(item.priceEuro).toFixed(2)}</p>
                    </div>
                `).join("")}
            </div>
        </article>
    `).join("");
}

function renderSelectedProject() {
    const project = getSelectedProject();

    if (!project) {
        elements.selectedProjectName.textContent = "Select a project";
        elements.selectedProjectStatus.textContent = "Idle";
        elements.selectedProjectIdea.textContent = "Pick a project to view the team, idea, and orders.";
        elements.selectedProjectDescription.textContent = "";
        elements.selectedProjectMeta.textContent = "";
        elements.selectedProjectMembers.innerHTML = "";
        elements.statusEditorSection.classList.add("hidden");
        elements.orderSheetSection.classList.add("hidden");
        renderOrderSheets(null);
        return;
    }

    elements.selectedProjectName.textContent = project.name;
    elements.selectedProjectStatus.textContent = project.status;
    elements.selectedProjectIdea.textContent = project.idea;
    elements.selectedProjectDescription.textContent = project.description;
    elements.selectedProjectMeta.textContent = `Created by ${project.createdByUsername} on ${new Date(project.createdAtUtc).toLocaleString()}`;
    elements.selectedProjectMembers.innerHTML = project.memberEmails
        .map((email) => `<span class="chip">${escapeHtml(email)}</span>`)
        .join("");

    if (project.canManageStatus) {
        elements.statusEditorSection.classList.remove("hidden");
        elements.projectStatusSelect.value = project.status;
    } else {
        elements.statusEditorSection.classList.add("hidden");
    }

    if (project.canCreateOrderSheet) {
        elements.orderSheetSection.classList.remove("hidden");
    } else {
        elements.orderSheetSection.classList.add("hidden");
    }

    renderOrderSheets(project);
}

function renderProjectDashboard() {
    const dashboard = state.dashboard;

    if (!dashboard) {
        elements.projectsList.innerHTML = '<p class="muted">Projects are unavailable right now.</p>';
        return;
    }

    if (!state.selectedProjectId || !dashboard.projects.some((project) => project.id === state.selectedProjectId)) {
        state.selectedProjectId = dashboard.projects[0]?.id ?? null;
    }

    const canCreate = dashboard.canCreateProject || hasPermission("projects.create");
    elements.openProjectModalButton.classList.toggle("hidden", !canCreate);
    elements.openProjectModalButtonAlt.classList.toggle("hidden", !canCreate);

    if (dashboard.canReviewGroupProjects) {
        elements.projectAccessHint.textContent = "You can review projects across the lab.";
    } else {
        elements.projectAccessHint.textContent = "You can see the projects you created or joined.";
    }

    renderProjectList();
    renderSelectedProject();
}

function renderUsers(users) {
    if (!users.length) {
        elements.usersList.innerHTML = '<p class="muted">No users found.</p>';
        return;
    }

    elements.usersList.innerHTML = users.map((user) => `
        <article class="stack-item">
            <h4>${escapeHtml(user.username)}</h4>
            <p>${escapeHtml(user.radboudEmail)}</p>
            <p>Student number: ${escapeHtml(user.studentNumber)}</p>
            <div class="chip-list">
                ${user.groups.map((group) => `<span class="chip">${escapeHtml(group)}</span>`).join("")}
            </div>
        </article>
    `).join("");
}

function renderGroups(groups) {
    elements.groupsList.innerHTML = groups.map((group) => `
        <article class="stack-item">
            <h4>${escapeHtml(group.name)}</h4>
            <p>${escapeHtml(group.description)}</p>
            <div class="chip-list">
                ${group.permissions.map((permission) => `<span class="chip">${escapeHtml(permission)}</span>`).join("")}
            </div>
            <p>${group.memberCount} member(s)</p>
        </article>
    `).join("");
}

function renderPermissions(permissions) {
    elements.permissionsList.innerHTML = permissions.length
        ? permissions.map((permission) => `<span class="chip">${escapeHtml(permission.code)}</span>`).join("")
        : '<p class="muted">No permissions found.</p>';
}

function renderCreateGroupOptions(groups) {
    elements.createGroupOptions.innerHTML = groups.map((group) => `
        <label>
            <input type="checkbox" name="groupIds" value="${group.id}">
            <span>${escapeHtml(group.name)}</span>
        </label>
    `).join("");
}

function renderMembershipUserSelect(users) {
    const previous = elements.membershipUserSelect.value;
    elements.membershipUserSelect.innerHTML = users.map((user) => `
        <option value="${user.id}">${escapeHtml(user.username)} (${escapeHtml(user.radboudEmail)})</option>
    `).join("");

    if (previous && users.some((user) => user.id === previous)) {
        elements.membershipUserSelect.value = previous;
    }
}

function renderMembershipGroupOptions() {
    const user = state.overview.users.find((item) => item.id === elements.membershipUserSelect.value);

    if (!user) {
        elements.membershipGroupOptions.innerHTML = '<p class="muted">Select a user.</p>';
        return;
    }

    const selectedNames = new Set(user.groups);
    elements.membershipGroupOptions.innerHTML = state.overview.groups.map((group) => `
        <label>
            <input type="checkbox" value="${group.id}" ${selectedNames.has(group.name) ? "checked" : ""}>
            <span>${escapeHtml(group.name)}</span>
        </label>
    `).join("");
}

function renderGroupPermissionSelect(groups) {
    const previous = elements.groupPermissionSelect.value;
    elements.groupPermissionSelect.innerHTML = groups.map((group) => `
        <option value="${group.id}">${escapeHtml(group.name)}</option>
    `).join("");

    if (previous && groups.some((group) => group.id === previous)) {
        elements.groupPermissionSelect.value = previous;
    }
}

function renderPermissionOptions() {
    const group = state.overview.groups.find((item) => item.id === elements.groupPermissionSelect.value);

    if (!group) {
        elements.permissionOptions.innerHTML = '<p class="muted">Select a group.</p>';
        return;
    }

    const selectedCodes = new Set(group.permissions);
    elements.permissionOptions.innerHTML = state.overview.permissions.map((permission) => `
        <label>
            <input type="checkbox" value="${permission.id}" ${selectedCodes.has(permission.code) ? "checked" : ""}>
            <span>${escapeHtml(permission.code)}</span>
        </label>
    `).join("");
}

function renderAdminOverview() {
    if (!state.overview) {
        return;
    }

    elements.usersStat.textContent = state.overview.users.length;
    elements.groupsStat.textContent = state.overview.groups.length;
    elements.permissionsStat.textContent = state.overview.permissions.length;

    renderUsers(state.overview.users);
    renderGroups(state.overview.groups);
    renderPermissions(state.overview.permissions);
    renderCreateGroupOptions(state.overview.groups);
    renderMembershipUserSelect(state.overview.users);
    renderMembershipGroupOptions();
    renderGroupPermissionSelect(state.overview.groups);
    renderPermissionOptions();
}

async function loadAdminOverview() {
    if (!hasPermission("admin.access")) {
        state.overview = null;
        return;
    }

    state.overview = await apiRequest("/api/admin/overview");
    renderAdminOverview();
}

async function loadProjectDashboard() {
    state.dashboard = await apiRequest("/api/projects");
    renderProjectDashboard();
}

async function syncCurrentUser() {
    state.currentUser = await apiRequest("/api/auth/me");
    renderUserShell();
}

async function bootstrap() {
    try {
        await syncCurrentUser();
        await Promise.all([loadProjectDashboard(), loadAdminOverview()]);
        showApp();
    } catch (error) {
        if (error.status !== 401) {
            elements.loginMessage.textContent = error.message;
            elements.loginMessage.classList.add("error");
        }

        showLogin();
    }
}

elements.loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.loginMessage.textContent = "Signing in...";
    elements.loginMessage.classList.remove("error");

    const formData = new FormData(elements.loginForm);

    try {
        state.currentUser = await apiRequest("/api/auth/login", {
            method: "POST",
            body: JSON.stringify({
                identifier: formData.get("identifier"),
                password: formData.get("password"),
                rememberMe: formData.get("rememberMe") === "on"
            })
        });

        elements.loginForm.reset();
        renderUserShell();
        await Promise.all([loadProjectDashboard(), loadAdminOverview()]);
        elements.loginMessage.textContent = "";
        showApp();
    } catch (error) {
        elements.loginMessage.textContent = error.message;
        elements.loginMessage.classList.add("error");
    }
});

elements.accountMenuButton.addEventListener("click", () => {
    elements.accountMenuPanel.classList.toggle("hidden");
});

elements.profileMenuButton.addEventListener("click", openProfilePanel);

elements.accountAdminButton.addEventListener("click", () => {
    closeAccountMenu();

    if (hasPermission("admin.access")) {
        activateSection("adminSection");
    }
});

elements.logoutButton.addEventListener("click", async () => {
    try {
        await apiRequest("/api/auth/logout", { method: "POST" });
    } finally {
        state.currentUser = null;
        state.overview = null;
        state.dashboard = null;
        state.selectedProjectId = null;
        showLogin();
    }
});

elements.closeProfileButton.addEventListener("click", closeProfilePanel);
elements.openProjectModalButton.addEventListener("click", openProjectModal);
elements.openProjectModalButtonAlt.addEventListener("click", openProjectModal);
elements.closeProjectModalButton.addEventListener("click", closeProjectModal);
elements.cancelProjectModalButton.addEventListener("click", closeProjectModal);

elements.overlay.addEventListener("click", () => {
    closeProfilePanel();
    closeProjectModal();
    closeAccountMenu();
});

elements.navLinks.forEach((button) => {
    button.addEventListener("click", () => activateSection(button.dataset.sectionTarget));
});

document.addEventListener("click", (event) => {
    if (!elements.accountMenuPanel.contains(event.target) && !elements.accountMenuButton.contains(event.target)) {
        closeAccountMenu();
    }
});

elements.profileForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.profileMessage.textContent = "Saving...";
    elements.profileMessage.classList.remove("error");

    const formData = new FormData(elements.profileForm);

    try {
        state.currentUser = await apiRequest("/api/auth/profile", {
            method: "PUT",
            body: JSON.stringify({
                username: formData.get("username")
            })
        });

        renderUserShell();
        elements.profileMessage.textContent = "Profile updated.";
    } catch (error) {
        elements.profileMessage.textContent = error.message;
        elements.profileMessage.classList.add("error");
    }
});

elements.passwordForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.passwordMessage.textContent = "Updating...";
    elements.passwordMessage.classList.remove("error");

    const formData = new FormData(elements.passwordForm);

    try {
        await apiRequest("/api/auth/change-password", {
            method: "POST",
            body: JSON.stringify({
                currentPassword: formData.get("currentPassword"),
                newPassword: formData.get("newPassword")
            })
        });

        elements.passwordForm.reset();
        elements.passwordMessage.textContent = "Password updated.";
    } catch (error) {
        elements.passwordMessage.textContent = error.message;
        elements.passwordMessage.classList.add("error");
    }
});

elements.projectPreviewForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.projectMessage.textContent = "Creating...";
    elements.projectMessage.classList.remove("error");

    const formData = new FormData(elements.projectPreviewForm);
    const rawEmails = String(formData.get("memberEmails") || "");
    const memberEmails = rawEmails
        .split(",")
        .map((email) => email.trim())
        .filter(Boolean);

    try {
        const created = await apiRequest("/api/projects", {
            method: "POST",
            body: JSON.stringify({
                name: formData.get("name"),
                idea: formData.get("idea"),
                description: formData.get("description"),
                memberEmails
            })
        });

        elements.projectPreviewForm.reset();
        closeProjectModal();
        await loadProjectDashboard();
        state.selectedProjectId = created.id;
        renderProjectDashboard();
        activateSection("projectsSection");
    } catch (error) {
        elements.projectMessage.textContent = error.message;
        elements.projectMessage.classList.add("error");
    }
});

elements.projectsList.addEventListener("click", (event) => {
    const button = event.target.closest("[data-project-id]");
    if (!button) {
        return;
    }

    state.selectedProjectId = button.dataset.projectId;
    renderProjectDashboard();
});

elements.saveProjectStatusButton.addEventListener("click", async () => {
    const project = getSelectedProject();
    if (!project) {
        return;
    }

    elements.projectStatusMessage.textContent = "Saving...";
    elements.projectStatusMessage.classList.remove("error");

    try {
        await apiRequest(`/api/projects/${project.id}/status`, {
            method: "PUT",
            body: JSON.stringify({
                status: elements.projectStatusSelect.value
            })
        });

        await loadProjectDashboard();
        state.selectedProjectId = project.id;
        renderProjectDashboard();
        elements.projectStatusMessage.textContent = "Status updated.";
    } catch (error) {
        elements.projectStatusMessage.textContent = error.message;
        elements.projectStatusMessage.classList.add("error");
    }
});

elements.addOrderItemButton.addEventListener("click", () => {
    elements.orderSheetItems.appendChild(createEmptyOrderItemRow());
});

elements.orderSheetItems.addEventListener("click", (event) => {
    const button = event.target.closest("[data-remove-order-item]");
    if (!button) {
        return;
    }

    button.closest(".order-item-grid")?.remove();
});

elements.orderSheetForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const project = getSelectedProject();

    if (!project) {
        return;
    }

    elements.orderSheetMessage.textContent = "Saving...";
    elements.orderSheetMessage.classList.remove("error");

    const items = [...elements.orderSheetItems.querySelectorAll(".order-item-grid")].map((row) => ({
        siteName: row.querySelector('[name="siteName"]').value,
        componentName: row.querySelector('[name="componentName"]').value,
        brand: row.querySelector('[name="brand"]').value,
        link: row.querySelector('[name="link"]').value,
        priceEuro: Number(row.querySelector('[name="priceEuro"]').value)
    }));

    try {
        await apiRequest(`/api/projects/${project.id}/order-sheets`, {
            method: "POST",
            body: JSON.stringify({
                items,
                submit: elements.orderSheetForm.querySelector('[name="submitNow"]').checked
            })
        });

        resetOrderSheetForm();
        await loadProjectDashboard();
        state.selectedProjectId = project.id;
        renderProjectDashboard();
        elements.orderSheetMessage.textContent = "Order sheet saved.";
    } catch (error) {
        elements.orderSheetMessage.textContent = error.message;
        elements.orderSheetMessage.classList.add("error");
    }
});

elements.membershipUserSelect.addEventListener("change", () => {
    if (state.overview) {
        renderMembershipGroupOptions();
    }
});

elements.groupPermissionSelect.addEventListener("change", () => {
    if (state.overview) {
        renderPermissionOptions();
    }
});

elements.createUserForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.createUserMessage.textContent = "Saving...";
    elements.createUserMessage.classList.remove("error");

    const formData = new FormData(elements.createUserForm);

    try {
        await apiRequest("/api/admin/users", {
            method: "POST",
            body: JSON.stringify({
                radboudEmail: formData.get("radboudEmail"),
                username: formData.get("username"),
                password: formData.get("password"),
                studentNumber: formData.get("studentNumber"),
                enrollmentDate: formData.get("enrollmentDate"),
                groupIds: selectedValues('#createGroupOptions input[type="checkbox"]')
            })
        });

        elements.createUserForm.reset();
        elements.createUserMessage.textContent = "User created.";
        await loadAdminOverview();
    } catch (error) {
        elements.createUserMessage.textContent = error.message;
        elements.createUserMessage.classList.add("error");
    }
});

elements.membershipForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.membershipMessage.textContent = "Saving...";
    elements.membershipMessage.classList.remove("error");

    try {
        await apiRequest(`/api/admin/users/${elements.membershipUserSelect.value}/groups`, {
            method: "PUT",
            body: JSON.stringify({
                groupIds: selectedValues('#membershipGroupOptions input[type="checkbox"]')
            })
        });

        elements.membershipMessage.textContent = "Memberships updated.";
        await loadAdminOverview();
    } catch (error) {
        elements.membershipMessage.textContent = error.message;
        elements.membershipMessage.classList.add("error");
    }
});

elements.groupPermissionForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.groupPermissionMessage.textContent = "Saving...";
    elements.groupPermissionMessage.classList.remove("error");

    try {
        await apiRequest(`/api/admin/groups/${elements.groupPermissionSelect.value}/permissions`, {
            method: "PUT",
            body: JSON.stringify({
                permissionIds: selectedValues('#permissionOptions input[type="checkbox"]')
            })
        });

        elements.groupPermissionMessage.textContent = "Permissions updated.";
        await loadAdminOverview();
    } catch (error) {
        elements.groupPermissionMessage.textContent = error.message;
        elements.groupPermissionMessage.classList.add("error");
    }
});

bootstrap();
