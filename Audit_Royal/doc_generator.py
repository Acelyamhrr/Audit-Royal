#!/usr/bin/env python3
"""
Générateur de documentation C# amélioré
Crée un site HTML interactif avec recherche, thème sombre, statistiques, etc.
"""

import xml.etree.ElementTree as ET
import os
import re
import json
from pathlib import Path
from html import escape
from collections import defaultdict

class DocGenerator:
    def __init__(self, xml_path, output_dir="documentation_html"):
        self.xml_path = xml_path
        self.output_dir = output_dir
        self.classes = {}
        self.namespaces = defaultdict(list)
        self.stats = {
            'total_classes': 0,
            'total_methods': 0,
            'total_properties': 0,
            'total_fields': 0
        }
        
    def parse_xml(self):
        """Parse le fichier XML de documentation"""
        try:
            tree = ET.parse(self.xml_path)
            root = tree.getroot()
            
            members = root.find('members')
            if members is None:
                print("Aucun membre trouvé dans le XML")
                return
            
            for member in members.findall('member'):
                name = member.get('name')
                if name:
                    self.process_member(name, member)
            
            # Calculer les statistiques
            self.calculate_stats()
                    
        except Exception as e:
            print(f"Erreur lors du parsing XML: {e}")
            
    def process_member(self, name, member):
        """Traite un membre de la documentation"""
        parts = name.split(':')
        if len(parts) != 2:
            return
            
        member_type = parts[0]
        full_name = parts[1]
        
        # Extraire le namespace et le nom de classe
        namespace = ""
        if '.' in full_name:
            parts_name = full_name.rsplit('.', 1)
            class_name = parts_name[0]
            member_name = parts_name[1] if len(parts_name) > 1 else None
            
            # Extraire le namespace
            if '.' in class_name:
                namespace = '.'.join(class_name.split('.')[:-1])
        else:
            class_name = full_name
            member_name = None
            
        # Initialiser la classe
        if class_name not in self.classes:
            self.classes[class_name] = {
                'name': class_name.split('.')[-1],  # Nom court
                'full_name': class_name,
                'namespace': namespace,
                'summary': '',
                'methods': [],
                'fields': [],
                'properties': [],
                'remarks': '',
                'example': ''
            }
            if namespace:
                self.namespaces[namespace].append(class_name)
        
        # Extraire la documentation
        summary = member.find('summary')
        summary_text = summary.text.strip() if summary is not None and summary.text else ""
        
        remarks = member.find('remarks')
        remarks_text = remarks.text.strip() if remarks is not None and remarks.text else ""
        
        example = member.find('example')
        example_text = example.text.strip() if example is not None and example.text else ""
        
        # Extraire les paramètres
        params = []
        for param in member.findall('param'):
            param_name = param.get('name', '')
            param_desc = param.text.strip() if param.text else ""
            params.append({'name': param_name, 'desc': param_desc})
        
        # Extraire le retour
        returns = member.find('returns')
        returns_text = returns.text.strip() if returns is not None and returns.text else ""
        
        # Classer le membre
        if member_type == 'T':
            self.classes[class_name]['summary'] = summary_text
            self.classes[class_name]['remarks'] = remarks_text
            self.classes[class_name]['example'] = example_text
        elif member_type == 'M':
            self.classes[class_name]['methods'].append({
                'name': member_name,
                'summary': summary_text,
                'params': params,
                'returns': returns_text,
                'remarks': remarks_text
            })
        elif member_type == 'F':
            self.classes[class_name]['fields'].append({
                'name': member_name,
                'summary': summary_text
            })
        elif member_type == 'P':
            self.classes[class_name]['properties'].append({
                'name': member_name,
                'summary': summary_text
            })
    
    def calculate_stats(self):
        """Calcule les statistiques"""
        self.stats['total_classes'] = len(self.classes)
        for class_data in self.classes.values():
            self.stats['total_methods'] += len(class_data['methods'])
            self.stats['total_properties'] += len(class_data['properties'])
            self.stats['total_fields'] += len(class_data['fields'])
    
    def generate_html(self):
        """Génère les fichiers HTML"""
        os.makedirs(self.output_dir, exist_ok=True)
        
        # Générer le fichier JSON pour la recherche
        self.generate_search_index()
        
        # Générer l'index
        self.generate_index()
        
        # Générer une page par classe
        for class_name, class_data in self.classes.items():
            self.generate_class_page(class_name, class_data)
        
        # Copier le CSS et JS
        self.generate_css()
        self.generate_js()
        
        print(f"\n✅ Documentation générée dans: {self.output_dir}/")
        print(f"📄 Ouvrez: {self.output_dir}/index.html")
        print(f"\n📊 Statistiques:")
        print(f"   • {self.stats['total_classes']} classes")
        print(f"   • {self.stats['total_methods']} méthodes")
        print(f"   • {self.stats['total_properties']} propriétés")
        print(f"   • {self.stats['total_fields']} champs")
    
    def generate_search_index(self):
        """Génère l'index de recherche JSON"""
        search_data = []
        for class_name, class_data in self.classes.items():
            safe_name = self.sanitize_filename(class_name)
            search_data.append({
                'name': class_data['name'],
                'full_name': class_data['full_name'],
                'namespace': class_data['namespace'],
                'summary': class_data['summary'],
                'url': f'{safe_name}.html',
                'type': 'class'
            })
            
            # Ajouter les méthodes
            for method in class_data['methods']:
                search_data.append({
                    'name': f"{class_data['name']}.{method['name']}",
                    'full_name': f"{class_data['full_name']}.{method['name']}",
                    'namespace': class_data['namespace'],
                    'summary': method['summary'],
                    'url': f'{safe_name}.html#{method["name"]}',
                    'type': 'method'
                })
        
        with open(os.path.join(self.output_dir, 'search-index.json'), 'w', encoding='utf-8') as f:
            json.dump(search_data, f, ensure_ascii=False, indent=2)
    
    def generate_index(self):
        """Génère la page d'index"""
        html = f"""<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Documentation - Audit Royal</title>
    <link rel="stylesheet" href="style.css">
</head>
<body>
    <header>
        <div class="header-content">
            <h1>📚 Documentation - Audit Royal</h1>
            <p>Documentation générée automatiquement à partir des commentaires XML</p>
            <button id="theme-toggle" class="theme-toggle" title="Changer de thème">
                <span class="theme-icon">🌙</span>
            </button>
        </div>
    </header>
    
    <div class="container">
        <nav class="sidebar">
            <div class="search-container">
                <input type="text" id="search-input" placeholder="🔍 Rechercher..." />
                <div id="search-results" class="search-results"></div>
            </div>
            
            <h2>Navigation</h2>
            <div class="nav-tabs">
                <button class="nav-tab active" data-tab="classes">Classes</button>
                <button class="nav-tab" data-tab="namespaces">Namespaces</button>
            </div>
            
            <div id="classes-tab" class="tab-content active">
                <ul class="class-list">
"""
        
        for class_name in sorted(self.classes.keys()):
            safe_name = self.sanitize_filename(class_name)
            short_name = self.classes[class_name]['name']
            namespace = self.classes[class_name]['namespace']
            namespace_label = f'<span class="namespace-label">{escape(namespace)}</span>' if namespace else ''
            html += f'                    <li><a href="{safe_name}.html">{escape(short_name)}</a>{namespace_label}</li>\n'
        
        html += """                </ul>
            </div>
            
            <div id="namespaces-tab" class="tab-content">
"""
        
        # Grouper par namespace
        for namespace in sorted(self.namespaces.keys()):
            html += f"""                <div class="namespace-group">
                    <h3>{escape(namespace) if namespace else 'Global'}</h3>
                    <ul class="class-list">
"""
            for class_name in sorted(self.namespaces[namespace]):
                safe_name = self.sanitize_filename(class_name)
                short_name = self.classes[class_name]['name']
                html += f'                        <li><a href="{safe_name}.html">{escape(short_name)}</a></li>\n'
            html += """                    </ul>
                </div>
"""
        
        html += """            </div>
        </nav>
        
        <main class="main-content">
            <div class="welcome-section">
                <h2>Bienvenue dans la documentation</h2>
                <p>Cette documentation contient toutes les classes, méthodes et propriétés du projet Audit Royal.</p>
                <p>Utilisez la barre de recherche ou naviguez par classes/namespaces dans le menu.</p>
            </div>
            
            <div class="stats-grid">
                <div class="stat-card">
                    <div class="stat-icon">📦</div>
                    <div class="stat-number">""" + str(self.stats['total_classes']) + """</div>
                    <div class="stat-label">Classes</div>
                </div>
                <div class="stat-card">
                    <div class="stat-icon">⚡</div>
                    <div class="stat-number">""" + str(self.stats['total_methods']) + """</div>
                    <div class="stat-label">Méthodes</div>
                </div>
                <div class="stat-card">
                    <div class="stat-icon">🔧</div>
                    <div class="stat-number">""" + str(self.stats['total_properties']) + """</div>
                    <div class="stat-label">Propriétés</div>
                </div>
                <div class="stat-card">
                    <div class="stat-icon">📝</div>
                    <div class="stat-number">""" + str(self.stats['total_fields']) + """</div>
                    <div class="stat-label">Champs</div>
                </div>
            </div>
        </main>
    </div>
    
    <footer>
        <p>Généré avec DocGenerator v2.0 pour C# • © 2024</p>
    </footer>
    
    <script src="script.js"></script>
</body>
</html>"""
        
        with open(os.path.join(self.output_dir, 'index.html'), 'w', encoding='utf-8') as f:
            f.write(html)
    
    def generate_class_page(self, class_name, class_data):
        """Génère la page d'une classe"""
        safe_name = self.sanitize_filename(class_name)
        
        html = f"""<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{escape(class_data['name'])} - Documentation</title>
    <link rel="stylesheet" href="style.css">
</head>
<body>
    <header>
        <div class="header-content">
            <h1>📚 Documentation - Audit Royal</h1>
            <p><a href="index.html" class="back-link">← Retour à l'index</a></p>
            <button id="theme-toggle" class="theme-toggle" title="Changer de thème">
                <span class="theme-icon">🌙</span>
            </button>
        </div>
    </header>
    
    <div class="container">
        <nav class="sidebar">
            <div class="search-container">
                <input type="text" id="search-input" placeholder="🔍 Rechercher..." />
                <div id="search-results" class="search-results"></div>
            </div>
            
            <h2>Navigation</h2>
            <ul class="class-list">
"""
        
        for cn in sorted(self.classes.keys()):
            safe = self.sanitize_filename(cn)
            short_name = self.classes[cn]['name']
            active = ' class="active"' if cn == class_name else ''
            html += f'                <li{active}><a href="{safe}.html">{escape(short_name)}</a></li>\n'
        
        html += f"""            </ul>
        </nav>
        
        <main class="main-content">
            <div class="class-header">
                <div class="breadcrumb">
                    <a href="index.html">Accueil</a>
                    {f'<span>→</span><span>{escape(class_data["namespace"])}</span>' if class_data['namespace'] else ''}
                    <span>→</span><span class="current">{escape(class_data['name'])}</span>
                </div>
                <h2 class="class-title">{escape(class_data['name'])}</h2>
                {f'<p class="namespace-info">Namespace: <code>{escape(class_data["namespace"])}</code></p>' if class_data['namespace'] else ''}
                <p class="class-summary">{escape(class_data['summary'])}</p>
                {f'<div class="remarks"><h4>Remarques</h4><p>{escape(class_data["remarks"])}</p></div>' if class_data['remarks'] else ''}
                {f'<div class="example"><h4>Exemple</h4><pre><code>{escape(class_data["example"])}</code></pre></div>' if class_data['example'] else ''}
            </div>
            
            <div class="member-summary">
                <h3>Résumé des membres</h3>
                <div class="summary-badges">
"""
        
        if class_data['fields']:
            html += f'                    <a href="#fields" class="badge badge-field">{len(class_data["fields"])} Champs</a>\n'
        if class_data['properties']:
            html += f'                    <a href="#properties" class="badge badge-property">{len(class_data["properties"])} Propriétés</a>\n'
        if class_data['methods']:
            html += f'                    <a href="#methods" class="badge badge-method">{len(class_data["methods"])} Méthodes</a>\n'
        
        html += """                </div>
            </div>
"""
        
        # Champs
        if class_data['fields']:
            html += """
            <section id="fields" class="member-section">
                <h3>🔹 Champs</h3>
                <div class="member-grid">
"""
            for field in class_data['fields']:
                html += f"""                    <div class="member-card">
                        <h4><code>{escape(field['name'])}</code></h4>
                        <p>{escape(field['summary'])}</p>
                    </div>
"""
            html += """                </div>
            </section>
"""
        
        # Propriétés
        if class_data['properties']:
            html += """
            <section id="properties" class="member-section">
                <h3>🔸 Propriétés</h3>
                <div class="member-grid">
"""
            for prop in class_data['properties']:
                html += f"""                    <div class="member-card">
                        <h4><code>{escape(prop['name'])}</code></h4>
                        <p>{escape(prop['summary'])}</p>
                    </div>
"""
            html += """                </div>
            </section>
"""
        
        # Méthodes
        if class_data['methods']:
            html += """
            <section id="methods" class="member-section">
                <h3>⚡ Méthodes</h3>
"""
            for method in class_data['methods']:
                method_id = self.sanitize_filename(method['name'])
                html += f"""
                <div class="method-card" id="{method_id}">
                    <div class="method-header">
                        <h4><code>{escape(method['name'])}</code></h4>
                        <a href="#{method_id}" class="anchor-link">#</a>
                    </div>
                    <p class="method-summary">{escape(method['summary'])}</p>
"""
                
                if method['params']:
                    html += """
                    <div class="params-section">
                        <h5>Paramètres</h5>
                        <table class="params-table">
"""
                    for param in method['params']:
                        html += f"""                            <tr>
                                <td><code>{escape(param["name"])}</code></td>
                                <td>{escape(param["desc"])}</td>
                            </tr>
"""
                    html += """                        </table>
                    </div>
"""
                
                if method['returns']:
                    html += f"""
                    <div class="returns-section">
                        <h5>Valeur de retour</h5>
                        <p>{escape(method['returns'])}</p>
                    </div>
"""
                
                if method['remarks']:
                    html += f"""
                    <div class="remarks-section">
                        <h5>Remarques</h5>
                        <p>{escape(method['remarks'])}</p>
                    </div>
"""
                
                html += """                </div>
"""
            html += """            </section>
"""
        
        html += """        </main>
    </div>
    
    <footer>
        <p>Généré avec DocGenerator v2.0 pour C# • © 2024</p>
    </footer>
    
    <script src="script.js"></script>
</body>
</html>"""
        
        with open(os.path.join(self.output_dir, f'{safe_name}.html'), 'w', encoding='utf-8') as f:
            f.write(html)
    
    def generate_css(self):
        """Génère le fichier CSS amélioré"""
        css = """/* Variables CSS pour le thème */
:root {
    --bg-primary: #ffffff;
    --bg-secondary: #f8f9fa;
    --bg-card: #ffffff;
    --text-primary: #2d3748;
    --text-secondary: #718096;
    --border-color: #e2e8f0;
    --accent-primary: #667eea;
    --accent-secondary: #764ba2;
    --shadow: 0 1px 3px rgba(0,0,0,0.1);
    --shadow-lg: 0 10px 30px rgba(0,0,0,0.1);
}

[data-theme="dark"] {
    --bg-primary: #1a202c;
    --bg-secondary: #2d3748;
    --bg-card: #2d3748;
    --text-primary: #e2e8f0;
    --text-secondary: #a0aec0;
    --border-color: #4a5568;
    --shadow: 0 1px 3px rgba(0,0,0,0.3);
    --shadow-lg: 0 10px 30px rgba(0,0,0,0.3);
}

* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
    line-height: 1.6;
    color: var(--text-primary);
    background: var(--bg-secondary);
    transition: background-color 0.3s ease, color 0.3s ease;
}

/* Header */
header {
    background: linear-gradient(135deg, var(--accent-primary) 0%, var(--accent-secondary) 100%);
    color: white;
    padding: 1.5rem 2rem;
    box-shadow: var(--shadow-lg);
    position: sticky;
    top: 0;
    z-index: 100;
}

.header-content {
    max-width: 1400px;
    margin: 0 auto;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

header h1 {
    font-size: 1.5rem;
    font-weight: 600;
}

header p {
    opacity: 0.9;
    font-size: 0.9rem;
    margin-top: 0.25rem;
}

.back-link {
    color: white;
    text-decoration: none;
    border-bottom: 2px solid rgba(255,255,255,0.3);
    transition: border-color 0.3s;
    font-size: 0.9rem;
}

.back-link:hover {
    border-bottom-color: white;
}

/* Theme Toggle */
.theme-toggle {
    background: rgba(255,255,255,0.2);
    border: none;
    color: white;
    padding: 0.5rem 1rem;
    border-radius: 8px;
    cursor: pointer;
    font-size: 1.2rem;
    transition: all 0.3s;
    backdrop-filter: blur(10px);
}

.theme-toggle:hover {
    background: rgba(255,255,255,0.3);
    transform: scale(1.05);
}

/* Container */
.container {
    max-width: 1400px;
    margin: 0 auto;
    display: grid;
    grid-template-columns: 300px 1fr;
    gap: 2rem;
    padding: 2rem;
    min-height: calc(100vh - 200px);
}

/* Sidebar */
.sidebar {
    background: var(--bg-card);
    padding: 1.5rem;
    border-radius: 12px;
    box-shadow: var(--shadow);
    height: fit-content;
    position: sticky;
    top: 100px;
    max-height: calc(100vh - 120px);
    overflow-y: auto;
}

.sidebar h2 {
    font-size: 1rem;
    text-transform: uppercase;
    color: var(--text-secondary);
    margin: 1.5rem 0 1rem 0;
    padding-bottom: 0.5rem;
    border-bottom: 2px solid var(--accent-primary);
}

/* Search */
.search-container {
    position: relative;
    margin-bottom: 1rem;
}

#search-input {
    width: 100%;
    padding: 0.75rem;
    border: 2px solid var(--border-color);
    border-radius: 8px;
    font-size: 0.9rem;
    background: var(--bg-secondary);
    color: var(--text-primary);
    transition: all 0.3s;
}

#search-input:focus {
    outline: none;
    border-color: var(--accent-primary);
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.search-results {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background: var(--bg-card);
    border: 2px solid var(--border-color);
    border-radius: 8px;
    margin-top: 0.5rem;
    max-height: 400px;
    overflow-y: auto;
    box-shadow: var(--shadow-lg);
    display: none;
    z-index: 50;
}

.search-results.active {
    display: block;
}

.search-result-item {
    padding: 0.75rem;
    border-bottom: 1px solid var(--border-color);
    cursor: pointer;
    transition: background 0.2s;
}

.search-result-item:hover {
    background: var(--bg-secondary);
}

.search-result-item:last-child {
    border-bottom: none;
}

.search-result-name {
    font-weight: 600;
    color: var(--accent-primary);
    margin-bottom: 0.25rem;
}

.search-result-type {
    font-size: 0.75rem;
    color: var(--text-secondary);
    text-transform: uppercase;
}

/* Tabs */
.nav-tabs {
    display: flex;
    gap: 0.5rem;
    margin-bottom: 1rem;
}

.nav-tab {
    flex: 1;
    padding: 0.5rem;
    border: none;
    background: var(--bg-secondary);
    color: var(--text-secondary);
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.85rem;
    font-weight: 500;
    transition: all 0.3s;
}

.nav-tab.active {
    background: var(--accent-primary);
    color: white;
}

.tab-content {
    display: none;
}

.tab-content.active {
    display: block;
}

/* Class List */
.class-list {
    list-style: none;
}

.class-list li {
    margin-bottom: 0.5rem;
    position: relative;
}

.class-list li.active > a {
    background: var(--accent-primary);
    color: white;
    font-weight: 600;
}

.class-list a {
    display: block;
    padding: 0.6rem;
    color: var(--text-primary);
    text-decoration: none;
    border-radius: 6px;
    transition: all 0.3s;
    font-size: 0.9rem;
}

.class-list a:hover {
    background: var(--bg-secondary);
    transform: translateX(3px);
}

.namespace-label {
    display: block;
    font-size: 0.7rem;
    color: var(--text-secondary);
    margin-top: 0.25rem;
    padding-left: 0.6rem;
}

.namespace-group {
    margin-bottom: 1.5rem;
}

.namespace-group h3 {
    font-size: 0.85rem;
    color: var(--accent-secondary);
    margin-bottom: 0.5rem;
    font-weight: 600;
}

/* Main Content */
.main-content {
    background: var(--bg-card);
    padding: 2rem;
    border-radius: 12px;
    box-shadow: var(--shadow);
}

.welcome-section {
    margin-bottom: 3rem;
}

.welcome-section h2 {
    color: var(--accent-primary);
    font-size: 2rem;
    margin-bottom: 1rem;
}

.welcome-section p {
    color: var(--text-secondary);
    font-size: 1.1rem;
    margin-bottom: 0.5rem;
}

/* Stats Grid */
.stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 1.5rem;
    margin: 2rem 0;
}

.stat-card {
    background: linear-gradient(135deg, var(--accent-primary) 0%, var(--accent-secondary) 100%);
    color: white;
    padding: 2rem;
    border-radius: 12px;
    text-align: center;
    box-shadow: var(--shadow-lg);
    transition: transform 0.3s;
}

.stat-card:hover {
    transform: translateY(-5px);
}

.stat-icon {
    font-size: 2.5rem;
    margin-bottom: 0.5rem;
}

.stat-number {
    font-size: 2.5rem;
    font-weight: bold;
    margin: 0.5rem 0;
}

.stat-label {
    font-size: 0.9rem;
    opacity: 0.9;
    text-transform: uppercase;
    letter-spacing: 1px;
}

/* Class Header */
.breadcrumb {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.9rem;
    color: var(--text-secondary);
    margin-bottom: 1rem;
}

.breadcrumb a {
    color: var(--accent-primary);
    text-decoration: none;
}

.breadcrumb .current {
    color: var(--text-primary);
    font-weight: 600;
}

.class-header {
    margin-bottom: 2rem;
    padding-bottom: 2rem;
    border-bottom: 3px solid var(--border-color);
}

.class-title {
    color: var(--accent-primary);
    font-size: 2.5rem;
    margin-bottom: 0.5rem;
}

.namespace-info {
    color: var(--text-secondary);
    margin-bottom: 1rem;
}

.class-summary {
    font-size: 1.1rem;
    color: var(--text-secondary);
    line-height: 1.8;
}

.remarks, .example {
    margin-top: 1.5rem;
    padding: 1rem;
    background: var(--bg-secondary);
    border-radius: 8px;
    border-left: 4px solid var(--accent-primary);
}

.remarks h4, .example h4 {
    color: var(--accent-secondary);
    font-size: 0.9rem;
    text-transform: uppercase;
    margin-bottom: 0.5rem;
}

/* Member Summary */
.member-summary {
    margin: 2rem 0;
}

.member-summary h3 {
    color: var(--text-primary);
    margin-bottom: 1rem;
}

.summary-badges {
    display: flex;
    gap: 1rem;
    flex-wrap: wrap;
}

.badge {
    display: inline-block;
    padding: 0.5rem 1rem;
    border-radius: 20px;
    font-size: 0.85rem;
    font-weight: 600;
    text-decoration: none;
    transition: transform 0.3s;
}

.badge:hover {
    transform: scale(1.05);
}

.badge-field {
    background: #48bb78;
    color: white;
}

.badge-property {
    background: #4299e1;
    color: white;
}

.badge-method {
    background: #ed8936;
    color: white;
}

/* Member Sections */
.member-section {
    margin: 3rem 0;
}

.member-section h3 {
    color: var(--accent-secondary);
    font-size: 1.8rem;
    margin-bottom: 1.5rem;
    padding-bottom: 0.5rem;
    border-bottom: 2px solid var(--border-color);
}

.member-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 1rem;
}

.member-card {
    padding: 1.5rem;
    background: var(--bg-secondary);
    border-radius: 8px;
    border-left: 4px solid var(--accent-primary);
    transition: all 0.3s;
}

.member-card:hover {
    box-shadow: var(--shadow-lg);
    transform: translateY(-2px);
}

.member-card h4 {
    color: var(--accent-primary);
    font-size: 1.1rem;
    margin-bottom: 0.5rem;
}

.member-card p {
    color: var(--text-secondary);
    font-size: 0.9rem;
}

/* Method Cards */
.method-card {
    background: var(--bg-secondary);
    padding: 1.5rem;
    margin: 1.5rem 0;
    border-radius: 12px;
    border-left: 4px solid var(--accent-primary);
    transition: all 0.3s;
}

.method-card:hover {
    box-shadow: var(--shadow-lg);
}

.method-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.75rem;
}

.method-header h4 {
    color: var(--accent-primary);
    font-size: 1.3rem;
}

.anchor-link {
    color: var(--text-secondary);
    text-decoration: none;
    opacity: 0;
    transition: opacity 0.3s;
}

.method-card:hover .anchor-link {
    opacity: 1;
}

.method-summary {
    color: var(--text-secondary);
    margin-bottom: 1rem;
}

.params-section, .returns-section, .remarks-section {
    margin-top: 1rem;
    padding: 1rem;
    background: var(--bg-card);
    border-radius: 8px;
}

.params-section h5, .returns-section h5, .remarks-section h5 {
    color: var(--accent-secondary);
    font-size: 0.85rem;
    text-transform: uppercase;
    margin-bottom: 0.5rem;
    letter-spacing: 0.5px;
}

.params-table {
    width: 100%;
    border-collapse: collapse;
}

.params-table tr {
    border-bottom: 1px solid var(--border-color);
}

.params-table td {
    padding: 0.75rem 0.5rem;
}

.params-table td:first-child {
    width: 30%;
    font-weight: 600;
}

code {
    background: var(--bg-secondary);
    padding: 0.2rem 0.5rem;
    border-radius: 4px;
    font-family: 'Monaco', 'Menlo', 'Courier New', monospace;
    font-size: 0.9em;
    color: var(--accent-primary);
}

pre {
    background: var(--bg-primary);
    padding: 1rem;
    border-radius: 8px;
    overflow-x: auto;
    border: 1px solid var(--border-color);
}

pre code {
    background: none;
    padding: 0;
    color: var(--text-primary);
}

/* Footer */
footer {
    background: var(--bg-card);
    color: var(--text-secondary);
    text-align: center;
    padding: 2rem;
    margin-top: 3rem;
    font-size: 0.9rem;
}

/* Scrollbar */
::-webkit-scrollbar {
    width: 10px;
    height: 10px;
}

::-webkit-scrollbar-track {
    background: var(--bg-secondary);
}

::-webkit-scrollbar-thumb {
    background: var(--border-color);
    border-radius: 5px;
}

::-webkit-scrollbar-thumb:hover {
    background: var(--text-secondary);
}

/* Responsive */
@media (max-width: 1024px) {
    .container {
        grid-template-columns: 1fr;
    }
    
    .sidebar {
        position: static;
        max-height: none;
    }
    
    .stats-grid {
        grid-template-columns: repeat(2, 1fr);
    }
}

@media (max-width: 640px) {
    .header-content {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
    }
    
    .stats-grid {
        grid-template-columns: 1fr;
    }
    
    .member-grid {
        grid-template-columns: 1fr;
    }
}
"""
        
        with open(os.path.join(self.output_dir, 'style.css'), 'w', encoding='utf-8') as f:
            f.write(css)
    
    def generate_js(self):
        """Génère le fichier JavaScript"""
        js = """// Thème
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
"""
        
        with open(os.path.join(self.output_dir, 'script.js'), 'w', encoding='utf-8') as f:
            f.write(js)
    
    def sanitize_filename(self, name):
        """Nettoie un nom pour en faire un nom de fichier valide"""
        name = re.sub(r'[<>:"/\\|?*]', '_', name)
        return name

def main():
    import sys
    
    if len(sys.argv) < 2:
        print("Usage: python3 doc_generator.py <chemin_vers_xml> [dossier_sortie]")
        print("\nExemple:")
        print("  python3 doc_generator.py Library/ScriptAssemblies/Assembly-CSharp.xml")
        print("  python3 doc_generator.py fichier.xml /home/user/docs")
        sys.exit(1)
    
    xml_path = sys.argv[1]
    output_dir = sys.argv[2] if len(sys.argv) > 2 else "documentation_html"
    
    if not os.path.exists(xml_path):
        print(f"❌ Erreur: Le fichier {xml_path} n'existe pas")
        sys.exit(1)
    
    print(f"📖 Génération de la documentation depuis: {xml_path}")
    print(f"📁 Dossier de sortie: {output_dir}")
    print()
    
    generator = DocGenerator(xml_path, output_dir)
    generator.parse_xml()
    
    if not generator.classes:
        print("⚠️  Aucune classe trouvée dans le XML")
        print("Vérifiez que le fichier XML contient bien des commentaires de documentation")
        sys.exit(1)
    
    generator.generate_html()
    
    print(f"\n🚀 Pour visualiser: xdg-open {output_dir}/index.html")

if __name__ == "__main__":
    main()
