// Thème
const themeToggle = document.getElementById('theme-toggle');
const html = document.documentElement;

// Charger le thème sauvegardé
const savedTheme = localStorage.getItem('theme') || 'light';
html.setAttribute('data-theme', savedTheme);
updateThemeIcon(savedTheme);

themeToggle?.addEventListener('click', () => {
    const currentTheme = html.getAttribute('data-theme');
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    html.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
    updateThemeIcon(newTheme);
});

function updateThemeIcon(theme) {
    const icon = document.querySelector('.theme-icon');
    if (icon) {
        icon.textContent = theme === 'light' ? '🌙' : '☀️';
    }
}

// Recherche
let searchIndex = [];
fetch('search-index.json')
    .then(res => res.json())
    .then(data => searchIndex = data)
    .catch(err => console.error('Erreur chargement index:', err));

const searchInput = document.getElementById('search-input');
const searchResults = document.getElementById('search-results');

searchInput?.addEventListener('input', (e) => {
    const query = e.target.value.toLowerCase().trim();
    
    if (query.length < 2) {
        searchResults.classList.remove('active');
        return;
    }
    
    const results = searchIndex.filter(item => 
        item.name.toLowerCase().includes(query) ||
        item.summary.toLowerCase().includes(query)
    ).slice(0, 10);
    
    if (results.length > 0) {
        searchResults.innerHTML = results.map(item => `
            <div class="search-result-item" onclick="window.location.href='${item.url}'">
                <div class="search-result-name">${escapeHtml(item.name)}</div>
                <div class="search-result-type">${item.type}</div>
            </div>
        `).join('');
        searchResults.classList.add('active');
    } else {
        searchResults.innerHTML = '<div class="search-result-item">Aucun résultat</div>';
        searchResults.classList.add('active');
    }
});

// Fermer les résultats en cliquant ailleurs
document.addEventListener('click', (e) => {
    if (!searchInput?.contains(e.target) && !searchResults?.contains(e.target)) {
        searchResults?.classList.remove('active');
    }
});

// Navigation par onglets
const navTabs = document.querySelectorAll('.nav-tab');
navTabs.forEach(tab => {
    tab.addEventListener('click', () => {
        const targetTab = tab.getAttribute('data-tab');
        
        // Retirer active de tous
        navTabs.forEach(t => t.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
        
        // Activer le bon
        tab.classList.add('active');
        document.getElementById(`${targetTab}-tab`)?.classList.add('active');
    });
});

// Smooth scroll pour les ancres
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        target?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
});

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
