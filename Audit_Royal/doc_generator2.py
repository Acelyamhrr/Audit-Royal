#!/usr/bin/env python3
"""
Générateur de documentation C# à partir des fichiers XML
Crée un site HTML style Javadoc
"""

import xml.etree.ElementTree as ET
import os
import re
from pathlib import Path
from html import escape

class DocGenerator:
    def __init__(self, xml_path, output_dir="documentation_html"):
        self.xml_path = xml_path
        self.output_dir = output_dir
        self.classes = {}
        
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
                    
        except Exception as e:
            print(f"Erreur lors du parsing XML: {e}")
            
    def process_member(self, name, member):
        """Traite un membre de la documentation"""
        # Format: T:ClassName ou M:ClassName.MethodName
        parts = name.split(':')
        if len(parts) != 2:
            return
            
        member_type = parts[0]
        full_name = parts[1]
        
        # Extraire le nom de la classe
        if '.' in full_name:
            class_name = full_name.rsplit('.', 1)[0]
            member_name = full_name.rsplit('.', 1)[1]
        else:
            class_name = full_name
            member_name = None
            
        # Initialiser la classe si nécessaire
        if class_name not in self.classes:
            self.classes[class_name] = {
                'name': class_name,
                'summary': '',
                'methods': [],
                'fields': [],
                'properties': []
            }
        
        # Extraire la documentation
        summary = member.find('summary')
        summary_text = summary.text.strip() if summary is not None and summary.text else ""
        
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
            # Type (classe)
            self.classes[class_name]['summary'] = summary_text
        elif member_type == 'M':
            # Méthode
            self.classes[class_name]['methods'].append({
                'name': member_name,
                'summary': summary_text,
                'params': params,
                'returns': returns_text
            })
        elif member_type == 'F':
            # Champ
            self.classes[class_name]['fields'].append({
                'name': member_name,
                'summary': summary_text
            })
        elif member_type == 'P':
            # Propriété
            self.classes[class_name]['properties'].append({
                'name': member_name,
                'summary': summary_text
            })
    
    def generate_html(self):
        """Génère les fichiers HTML"""
        os.makedirs(self.output_dir, exist_ok=True)
        
        # Générer l'index
        self.generate_index()
        
        # Générer une page par classe
        for class_name, class_data in self.classes.items():
            self.generate_class_page(class_name, class_data)
        
        # Copier le CSS
        self.generate_css()
        
        print(f"\n✅ Documentation générée dans: {self.output_dir}/")
        print(f"📄 Ouvrez: {self.output_dir}/index.html")
    
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
        <h1>📚 Documentation - Audit Royal</h1>
        <p>Documentation générée automatiquement à partir des commentaires XML</p>
    </header>
    
    <nav>
        <h2>Classes</h2>
        <ul class="class-list">
"""
        
        for class_name in sorted(self.classes.keys()):
            safe_name = self.sanitize_filename(class_name)
            html += f'            <li><a href="{safe_name}.html">{escape(class_name)}</a></li>\n'
        
        html += """        </ul>
    </nav>
    
    <main>
        <h2>Vue d'ensemble</h2>
        <p>Sélectionnez une classe dans le menu de navigation pour voir sa documentation détaillée.</p>
        
        <div class="stats">
            <div class="stat-box">
                <div class="stat-number">""" + str(len(self.classes)) + """</div>
                <div class="stat-label">Classes</div>
            </div>
        </div>
    </main>
    
    <footer>
        <p>Généré avec DocGenerator pour C#</p>
    </footer>
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
    <title>{escape(class_name)} - Documentation</title>
    <link rel="stylesheet" href="style.css">
</head>
<body>
    <header>
        <h1>📚 Documentation - Audit Royal</h1>
        <p><a href="index.html">← Retour à l'index</a></p>
    </header>
    
    <nav>
        <h2>Classes</h2>
        <ul class="class-list">
"""
        
        for cn in sorted(self.classes.keys()):
            safe = self.sanitize_filename(cn)
            active = ' class="active"' if cn == class_name else ''
            html += f'            <li{active}><a href="{safe}.html">{escape(cn)}</a></li>\n'
        
        html += f"""        </ul>
    </nav>
    
    <main>
        <div class="class-header">
            <h2>{escape(class_name)}</h2>
            <p class="class-summary">{escape(class_data['summary'])}</p>
        </div>
"""
        
        # Champs
        if class_data['fields']:
            html += """
        <section class="member-section">
            <h3>Champs</h3>
            <table class="member-table">
                <thead>
                    <tr>
                        <th>Nom</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
"""
            for field in class_data['fields']:
                html += f"""                    <tr>
                        <td><code>{escape(field['name'])}</code></td>
                        <td>{escape(field['summary'])}</td>
                    </tr>
"""
            html += """                </tbody>
            </table>
        </section>
"""
        
        # Propriétés
        if class_data['properties']:
            html += """
        <section class="member-section">
            <h3>Propriétés</h3>
            <table class="member-table">
                <thead>
                    <tr>
                        <th>Nom</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
"""
            for prop in class_data['properties']:
                html += f"""                    <tr>
                        <td><code>{escape(prop['name'])}</code></td>
                        <td>{escape(prop['summary'])}</td>
                    </tr>
"""
            html += """                </tbody>
            </table>
        </section>
"""
        
        # Méthodes
        if class_data['methods']:
            html += """
        <section class="member-section">
            <h3>Méthodes</h3>
"""
            for method in class_data['methods']:
                html += f"""
            <div class="method-detail">
                <h4>{escape(method['name'])}</h4>
                <p class="method-summary">{escape(method['summary'])}</p>
"""
                
                if method['params']:
                    html += """
                <div class="params">
                    <h5>Paramètres:</h5>
                    <ul>
"""
                    for param in method['params']:
                        html += f'                        <li><code>{escape(param["name"])}</code> - {escape(param["desc"])}</li>\n'
                    html += """                    </ul>
                </div>
"""
                
                if method['returns']:
                    html += f"""
                <div class="returns">
                    <h5>Retourne:</h5>
                    <p>{escape(method['returns'])}</p>
                </div>
"""
                
                html += """            </div>
"""
            html += """        </section>
"""
        
        html += """    </main>
    
    <footer>
        <p>Généré avec DocGenerator pour C#</p>
    </footer>
</body>
</html>"""
        
        with open(os.path.join(self.output_dir, f'{safe_name}.html'), 'w', encoding='utf-8') as f:
            f.write(html)
    
    def generate_css(self):
        """Génère le fichier CSS"""
        css = """* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
    line-height: 1.6;
    color: #333;
    background: #f5f5f5;
    display: grid;
    grid-template-columns: 250px 1fr;
    grid-template-rows: auto 1fr auto;
    min-height: 100vh;
}

header {
    grid-column: 1 / -1;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 2rem;
    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

header h1 {
    font-size: 1.8rem;
    margin-bottom: 0.5rem;
}

header p {
    opacity: 0.9;
}

header a {
    color: white;
    text-decoration: none;
    border-bottom: 2px solid rgba(255,255,255,0.3);
    transition: border-color 0.3s;
}

header a:hover {
    border-bottom-color: white;
}

nav {
    background: white;
    padding: 2rem 1rem;
    border-right: 1px solid #e0e0e0;
    overflow-y: auto;
}

nav h2 {
    font-size: 1rem;
    text-transform: uppercase;
    color: #666;
    margin-bottom: 1rem;
    padding-bottom: 0.5rem;
    border-bottom: 2px solid #667eea;
}

.class-list {
    list-style: none;
}

.class-list li {
    margin-bottom: 0.5rem;
}

.class-list li.active {
    background: #f0f0f0;
    border-radius: 4px;
}

.class-list a {
    display: block;
    padding: 0.5rem;
    color: #333;
    text-decoration: none;
    border-radius: 4px;
    transition: all 0.3s;
}

.class-list a:hover {
    background: #667eea;
    color: white;
    transform: translateX(5px);
}

main {
    padding: 2rem;
    background: white;
    max-width: 1200px;
}

.class-header {
    margin-bottom: 2rem;
    padding-bottom: 1rem;
    border-bottom: 3px solid #667eea;
}

.class-header h2 {
    color: #667eea;
    font-size: 2rem;
    margin-bottom: 1rem;
}

.class-summary {
    font-size: 1.1rem;
    color: #666;
}

.member-section {
    margin: 2rem 0;
}

.member-section h3 {
    color: #764ba2;
    margin-bottom: 1rem;
    padding-bottom: 0.5rem;
    border-bottom: 2px solid #e0e0e0;
}

.member-table {
    width: 100%;
    border-collapse: collapse;
    margin: 1rem 0;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.member-table th,
.member-table td {
    padding: 1rem;
    text-align: left;
    border-bottom: 1px solid #e0e0e0;
}

.member-table th {
    background: #667eea;
    color: white;
    font-weight: 600;
}

.member-table tr:hover {
    background: #f9f9f9;
}

.member-table code {
    background: #f4f4f4;
    padding: 0.2rem 0.5rem;
    border-radius: 3px;
    font-family: 'Courier New', monospace;
    color: #667eea;
    font-weight: bold;
}

.method-detail {
    background: #f9f9f9;
    padding: 1.5rem;
    margin: 1rem 0;
    border-radius: 8px;
    border-left: 4px solid #667eea;
}

.method-detail h4 {
    color: #667eea;
    font-size: 1.3rem;
    margin-bottom: 0.5rem;
}

.method-summary {
    color: #666;
    margin-bottom: 1rem;
}

.params, .returns {
    margin-top: 1rem;
}

.params h5, .returns h5 {
    color: #764ba2;
    font-size: 0.9rem;
    text-transform: uppercase;
    margin-bottom: 0.5rem;
}

.params ul {
    list-style: none;
    padding-left: 1rem;
}

.params li {
    margin: 0.5rem 0;
}

.params code {
    background: white;
    padding: 0.2rem 0.5rem;
    border-radius: 3px;
    font-family: 'Courier New', monospace;
    color: #667eea;
    font-weight: bold;
}

.stats {
    display: flex;
    gap: 2rem;
    margin: 2rem 0;
}

.stat-box {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 2rem;
    border-radius: 10px;
    text-align: center;
    box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
}

.stat-number {
    font-size: 3rem;
    font-weight: bold;
}

.stat-label {
    font-size: 1rem;
    opacity: 0.9;
    text-transform: uppercase;
    letter-spacing: 1px;
}

footer {
    grid-column: 1 / -1;
    background: #333;
    color: white;
    text-align: center;
    padding: 1rem;
    font-size: 0.9rem;
}

@media (max-width: 768px) {
    body {
        grid-template-columns: 1fr;
    }
    
    nav {
        border-right: none;
        border-bottom: 1px solid #e0e0e0;
    }
}
"""
        
        with open(os.path.join(self.output_dir, 'style.css'), 'w', encoding='utf-8') as f:
            f.write(css)
    
    def sanitize_filename(self, name):
        """Nettoie un nom pour en faire un nom de fichier valide"""
        # Remplacer les caractères problématiques
        name = re.sub(r'[<>:"/\\|?*]', '_', name)
        return name

def main():
    import sys
    
    if len(sys.argv) < 2:
        print("Usage: python3 doc_generator.py <chemin_vers_xml> [dossier_sortie]")
        print("\nExemple:")
        print("  python3 doc_generator.py Library/ScriptAssemblies/Assembly-CSharp.xml")
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
    
    print(f"✅ {len(generator.classes)} classe(s) trouvée(s)")
    generator.generate_html()
    
    print(f"\n🚀 Pour visualiser: xdg-open {output_dir}/index.html")

if __name__ == "__main__":
    main()
