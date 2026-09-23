#!/usr/bin/env python3
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
errors = []

API_WHITELIST_TYPES = (
    "RHAH_Api",
    "IRHAH_Pawn",
    "RHAH_PawnSnapshot",
    "IRHAH_PawnBehavior",
    "RHAH_PawnBehaviors",
    "RHAH_PawnSeed",
    "RHAH_BehaviorGate",
    "RHAH_PawnRole",
    "RHAH_Lifecycle",
    "RHAH_ReleaseReason",
    "RHAH_Attitude",
)

TYPE_DECL = re.compile(
    r"\b(?:(?:public|internal|private|protected|sealed|static|partial|abstract|readonly)\s+)*"
    r"(?:class|interface|enum|struct|record)\s+([A-Za-z_][A-Za-z0-9_]*)"
)

GATE_NAMES = (
    "Beg",
    "Steal",
    "Fight",
    "LeaveAfterFed",
    "EatOutsideRelief",
    "FeedFromRelief",
    "Gnaw",
    "TailBite",
    "Leash",
    "Carry",
    "JoinColony",
    "Hire",
    "Transfer",
    "Imprison",
    "DropOffChild",
    "ExitMap",
)

API_METHODS = (
    "TryMarkOrigin",
    "SetGate",
    "SetExtra",
    "TryGetExtra",
    "IsOrigin",
    "IsVisitor",
    "IsRatkin",
    "IsRatkinYoung",
    "Allows",
    "GateQueried",
)

COMP_SAVE_FIELDS = (
    "spawnBatchId",
    "relationshipGroupId",
    "carriesPlague",
    "attitudeAtArrival",
    "hasBeenFed",
    "leaveAfterGameTick",
    "parentPawnLoadId",
    "childPawnLoadIds",
    "gateOverrides",
    "extraData",
)

PLACEHOLDER = re.compile(r"\{[^{}]+\}")
KEYED_ENTRY = re.compile(r"<([A-Za-z_][A-Za-z0-9_]*)>((?:(?!</\1>).)*)</\1>")
COMPILE_REMOVE = re.compile(r'<Compile\s+Remove\s*=\s*"([^"]+)"')
PROJECT_REF = re.compile(r'<ProjectReference\s+Include\s*=\s*"([^"]+)"')
TRANSLATE_LITERAL = re.compile(r'"(RHAH_[A-Za-z0-9_]+)"\s*\.Translate\s*\(')
SCRIBE_KEY = re.compile(
    r'Scribe_(?:Values|Collections|Deep|References)\.Look\s*\([^;]*?"([A-Za-z][A-Za-z0-9_]*)"'
)
DEF_BLOCK = re.compile(r"<(?P<kind>[A-Za-z0-9_]+Def)\b[^>]*>(?P<body>.*?)</(?P=kind)>", re.DOTALL)
DEF_NAME = re.compile(r"<defName>\s*([^<]+?)\s*</defName>")
CJK = re.compile(r"[\u4e00-\u9fff]")
INJECTED_FIELD = re.compile(r"<([A-Za-z0-9_]+(?:\.[A-Za-z0-9_]+)+)>")
COMMENT = re.compile(r"<!--.*?-->", re.DOTALL)

TEXT_FIELDS = ("label", "description", "reportString", "jobString", "fixedName")
BACKSTORY_FIELDS = {
    "title": "title",
    "titleShort": "titleShort",
    "description": "baseDescription",
}
LIST_FIELDS = ("label", "customEffectDescriptions")
DEF_ROOTS = ("1.6/Defs", "Biotech/Defs")
SKIP_DIRS = {".git", "tmp", "docs", "bin", "obj"}
SKIP_FILES = {"NOTICE", "README.md", "Agents.md", "AGENTS.md", "verify-scaffold.py"}


def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append("missing " + rel)
        return ""
    return path.read_text(encoding="utf-8")


def read_cs(rel_dir):
    folder = ROOT / rel_dir
    if not folder.is_dir():
        errors.append("missing " + rel_dir)
        return ""
    chunks = []
    for path in sorted(folder.rglob("*.cs")):
        if path.is_file():
            chunks.append(path.read_text(encoding="utf-8"))
    return "\n".join(chunks)


def strip_comments(text):
    return COMMENT.sub("", text)


def keyed_entries(rel_dir):
    folder = ROOT / rel_dir
    entries = {}
    if not folder.is_dir():
        errors.append("missing " + rel_dir)
        return entries
    for path in sorted(folder.rglob("*.xml")):
        if "DefInjected" in path.parts:
            continue
        text = strip_comments(path.read_text(encoding="utf-8"))
        for match in KEYED_ENTRY.finditer(text):
            name = match.group(1)
            if name != "LanguageData":
                entries[name] = match.group(2)
    return entries


def check_about():
    about = read("About/About.xml")
    if "nanaloveyuki.ratkin.hungerandhavoc" not in about:
        errors.append("About.xml packageId")
    if "鼠族: 饥与祸" not in about:
        errors.append("About.xml Chinese name")
    if "lezhizhong.mouse.disaster.famine" not in about:
        errors.append("About.xml incompatibleWith original")
    if "<modVersion>0.1.0</modVersion>" not in about:
        errors.append("About.xml modVersion must be 0.1.0")
    if (ROOT / "1.6/Assemblies/0Harmony.dll").exists():
        errors.append("do not ship 1.6/Assemblies/0Harmony.dll; Harmony is a mod dependency")


def check_load_folders():
    load = read("LoadFolders.xml")
    if "<li>Guard</li>" not in load or ">1.6</li>" not in load:
        errors.append("LoadFolders.xml must load Guard and 1.6")
    if "nanaloveyuki.mouse.disaster.famine.continued" not in load:
        errors.append("LoadFolders.xml must skip body when old mods are active")


def catalog_defs(catalog):
    ids = re.findall(r'"(I-\d{3})"', catalog)
    defs = re.findall(
        r'(?:Original|Sequel)\(\s*"I-\d{3}",\s*"(RHAH_[A-Za-z0-9_]+)"',
        catalog,
    )
    return ids, defs


def check_catalog():
    catalog = read("Source/Incidents/RHAH_IncidentCatalog.cs")
    ids, defs = catalog_defs(catalog)
    if len(ids) != 51:
        errors.append("catalog size %s, expected 51" % len(ids))
    if len(set(ids)) != len(ids):
        errors.append("duplicate display ids")
    expected = ["I-%03d" % i for i in range(1, 52)]
    if ids != expected:
        errors.append("display ids are not I-001..I-051 in order: %s" % ids[:8])
    if len(defs) != 51:
        errors.append("catalog def name count %s, expected 51" % len(defs))
    if "RHH_" in catalog:
        errors.append("catalog still has RHH_")
    return defs


def check_incident_labels(defs):
    zh_incidents = read("Languages/ChineseSimplified/Keyed/RHAH_Incidents.xml")
    en_incidents = read("Languages/English/Keyed/RHAH_Incidents.xml")
    for display_def in defs:
        key = "<RHAH_Incident_%s_Label>" % display_def[len("RHAH_"):]
        if key not in zh_incidents or key not in en_incidents:
            errors.append("missing label " + key)


def compare_keyed(zh_dir, en_dir, label):
    zh = keyed_entries(zh_dir)
    en = keyed_entries(en_dir)
    zh_keys = set(zh)
    en_keys = set(en)
    for key in sorted(zh_keys - en_keys):
        errors.append("%s English missing key %s" % (label, key))
    for key in sorted(en_keys - zh_keys):
        errors.append("%s ChineseSimplified missing key %s" % (label, key))
    for key in sorted(zh_keys & en_keys):
        zh_count = len(PLACEHOLDER.findall(zh[key]))
        en_count = len(PLACEHOLDER.findall(en[key]))
        if zh_count != en_count:
            errors.append(
                "%s placeholder count mismatch %s zh=%s en=%s"
                % (label, key, zh_count, en_count)
            )


def check_translate_literals():
    keys = keyed_entries("Languages/English/Keyed")
    for path in sorted((ROOT / "Source").rglob("*.cs")):
        if "Tests" in path.parts:
            continue
        text = path.read_text(encoding="utf-8")
        for key in sorted(set(TRANSLATE_LITERAL.findall(text))):
            if key not in keys:
                rel = str(path.relative_to(ROOT))
                errors.append("Translate key missing %s in %s" % (key, rel))


def compile_remove_excludes_api(csproj):
    for raw in COMPILE_REMOVE.findall(csproj):
        pattern = raw.replace("\\", "/").lower()
        if pattern in {"api/**/*.cs", "api/**/*", "api/**"}:
            return True
        if pattern.startswith("api/") and "**" in pattern and pattern.endswith(".cs"):
            return True
    return False


def has_api_project_reference(csproj):
    for raw in PROJECT_REF.findall(csproj):
        if raw.replace("\\", "/").endswith("HungerAndHavoc.Api.csproj"):
            return True
    return False


def has_impl_project_reference(csproj):
    for raw in PROJECT_REF.findall(csproj):
        if Path(raw.replace("\\", "/")).name == "HungerAndHavoc.csproj":
            return True
    return False


def check_projects():
    if not (ROOT / "Source/Api/HungerAndHavoc.Api.csproj").exists():
        errors.append("missing Source/Api/HungerAndHavoc.Api.csproj")
    else:
        api_csproj = read("Source/Api/HungerAndHavoc.Api.csproj")
        if has_impl_project_reference(api_csproj):
            errors.append("API csproj must not ProjectReference HungerAndHavoc.csproj")

    impl_csproj = read("Source/HungerAndHavoc.csproj")
    if impl_csproj:
        if not compile_remove_excludes_api(impl_csproj):
            errors.append(r"HungerAndHavoc.csproj must Compile Remove Api\**\*.cs")
        if not has_api_project_reference(impl_csproj):
            errors.append(
                "HungerAndHavoc.csproj must ProjectReference HungerAndHavoc.Api.csproj"
            )


def check_api_surface():
    api_sources = read_cs("Source/Api")
    declared = set(TYPE_DECL.findall(api_sources))
    for name in API_WHITELIST_TYPES:
        if name not in declared:
            errors.append("Source/Api missing type " + name)
    if not re.search(r"\bRegisterRatkinMatcher\b", api_sources):
        errors.append("Source/Api missing RegisterRatkinMatcher")
    for name in GATE_NAMES:
        if name not in api_sources:
            errors.append("missing gate " + name)
    for token in API_METHODS:
        if token not in api_sources:
            errors.append("API missing " + token)


def check_comp_contract():
    comp_rel = "Source/Identity/CompRHAH_Pawn.cs"
    comp_path = ROOT / comp_rel
    if not comp_path.exists():
        errors.append("missing " + comp_rel)
        return
    comp = comp_path.read_text(encoding="utf-8")
    if not re.search(r"namespace\s+HungerAndHavoc\.Identity\b", comp):
        errors.append("CompRHAH_Pawn must be in HungerAndHavoc.Identity")
    if re.search(r'"sourceIncidentId"', comp):
        errors.append("CompRHAH_Pawn still uses save key sourceIncidentId")
    if not re.search(r'"sourceIncidentDisplayId"', comp):
        errors.append(
            "CompRHAH_Pawn CompExposeData missing save key sourceIncidentDisplayId"
        )
    for token in COMP_SAVE_FIELDS:
        if token not in comp:
            errors.append("CompRHAH_Pawn missing " + token)


def check_identity_defs():
    hediff = read("1.6/Defs/HediffDefs/RHAH_HungerMark.xml")
    if "RHAH_HungerMark" not in hediff:
        errors.append("Hunger mark missing defName RHAH_HungerMark")
    if "HungerAndHavoc.Identity.Hediff_RHAH_Mark" not in hediff:
        errors.append(
            "Hunger mark hediffClass must be HungerAndHavoc.Identity.Hediff_RHAH_Mark"
        )
    genes = read("Biotech/Defs/GeneDefs/RHAH_Genes.xml")
    xenotypes = read("Biotech/Defs/GeneDefs/RHAH_Xenotypes.xml")
    if "RHAH_Gene_ThinRations" not in genes or "canGenerateInGeneSet>false" not in genes:
        errors.append("Owned hunger gene must stay out of random gene sets")
    if "RHAH_Xenotype_Ratkin" not in xenotypes:
        errors.append("Fallback xenotype RHAH_Xenotype_Ratkin is missing")
    if "HungerAndHavoc.Identity.CompProperties_RHAH_Pawn" not in hediff:
        errors.append(
            "Hunger mark Comp Class must be HungerAndHavoc.Identity.CompProperties_RHAH_Pawn"
        )


def injected_paths():
    found = set()
    folder = ROOT / "Languages/English/DefInjected"
    if not folder.is_dir():
        errors.append("missing Languages/English/DefInjected")
        return found
    for path in folder.rglob("*.xml"):
        text = strip_comments(path.read_text(encoding="utf-8"))
        found.update(INJECTED_FIELD.findall(text))
    return found



def list_item_paths(body, field):
    paths = []
    index = 0
    for item in re.findall(r"<li\b[^>]*>(.*?)</li>", body, flags=re.DOTALL):
        if ("<%s>" % field in item) or (field == "customEffectDescriptions" and CJK.search(item)):
            paths.append("%s.%s" % (field, index))
            index += 1
    return paths


def player_text_paths(kind, body):
    paths = []
    if kind == "BackstoryDef":
        for source, injected in BACKSTORY_FIELDS.items():
            match = re.search(r"<%s>(.*?)</%s>" % (source, source), body, flags=re.DOTALL)
            if match and CJK.search(match.group(1)):
                paths.append(injected)
        return paths

    for field in TEXT_FIELDS:
        match = re.search(r"<%s>(.*?)</%s>" % (field, field), body, flags=re.DOTALL)
        if match and CJK.search(match.group(1)):
            paths.append(field)
    stages = re.search(r"<stages>(.*?)</stages>", body, flags=re.DOTALL)
    if stages:
        for path in list_item_paths(stages.group(1), "label"):
            paths.append("stages." + path.replace("label.", "", 1) + ".label")
    for field in LIST_FIELDS:
        if field == "label":
            continue
        container = re.search(
            r"<%s>(.*?)</%s>" % (field, field), body, flags=re.DOTALL
        )
        if container:
            paths.extend(list_item_paths(container.group(1), field))
    return paths


def check_def_injected():
    injected = injected_paths()
    for root_name in DEF_ROOTS:
        root = ROOT / root_name
        if not root.is_dir():
            continue
        for path in sorted(root.rglob("*.xml")):
            text = strip_comments(path.read_text(encoding="utf-8"))
            for block in DEF_BLOCK.finditer(text):
                body = block.group("body")
                name_match = DEF_NAME.search(body)
                if name_match is None:
                    continue
                def_name = name_match.group(1)
                for field_path in player_text_paths(block.group("kind"), body):
                    key = def_name + "." + field_path
                    if key not in injected:
                        rel = str(path.relative_to(ROOT))
                        errors.append("English DefInjected missing %s (%s)" % (key, rel))


def check_save_keys():
    ownership = read("docs/save-ownership.md")
    for path in sorted((ROOT / "Source").rglob("*.cs")):
        if "Tests" in path.parts:
            continue
        text = path.read_text(encoding="utf-8")
        for key in sorted(set(SCRIBE_KEY.findall(text))):
            if key not in ownership:
                rel = str(path.relative_to(ROOT))
                errors.append("save key %s is not in save-ownership.md (%s)" % (key, rel))


def check_forbidden_names():
    for path in ROOT.rglob("*"):
        if not path.is_file() or any(part in SKIP_DIRS for part in path.parts):
            continue
        if path.name in SKIP_FILES or path.suffix.lower() not in {".cs", ".xml", ".md", ".ps1"}:
            continue
        text = path.read_text(encoding="utf-8", errors="ignore")
        rel = str(path.relative_to(ROOT))
        if "MouseDisaster" in text:
            errors.append("MouseDisaster leftover in " + rel)
        if re.search(r"\bRHH_", text):
            errors.append("RHH_ leftover in " + rel)
        if re.search(r"\b(?:RatkinEgg|IsEgg|RHAH_PawnRole\.Egg)\b", text):
            errors.append("ambiguous egg naming in " + rel)


def check_required_files():
    if not (ROOT / "docs/adr/0001-rewrite-design.md").exists():
        errors.append("missing adr 0001")
    if not (ROOT / "docs/adr/0002-pawn-mod-compatibility.md").exists():
        errors.append("missing adr 0002")
    if not (ROOT / "1.6/Assemblies").is_dir():
        errors.append("missing 1.6/Assemblies")


def main():
    check_about()
    check_load_folders()
    defs = check_catalog()
    check_incident_labels(defs)
    compare_keyed(
        "Languages/ChineseSimplified/Keyed",
        "Languages/English/Keyed",
        "Languages Keyed",
    )
    compare_keyed(
        "Guard/Languages/ChineseSimplified/Keyed",
        "Guard/Languages/English/Keyed",
        "Guard Keyed",
    )
    check_translate_literals()
    check_projects()
    check_api_surface()
    check_comp_contract()
    check_identity_defs()
    check_def_injected()
    check_save_keys()
    check_forbidden_names()
    check_required_files()

    if errors:
        print("FAIL")
        for item in errors:
            print(" -", item)
        return 1
    print("OK M0 scaffold: API assembly, Identity Comp, I-001..I-051, keyed symmetry")
    return 0


if __name__ == "__main__":
    sys.exit(main())
