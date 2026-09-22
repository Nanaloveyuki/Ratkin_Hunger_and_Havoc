#!/usr/bin/env python3
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
errors = []

API_WHITELIST_TYPES = (
    "HungerAndHavocApi",
    "IHungerPawn",
    "HungerPawnSnapshot",
    "IHungerPawnBehavior",
    "HungerPawnBehaviors",
    "HungerPawnSeed",
    "HungerBehaviorGate",
    "HungerPawnRole",
    "HungerLifecycle",
    "HungerReleaseReason",
    "HungerAttitude",
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
KEYED_ENTRY = re.compile(r"<([A-Za-z_][\w.]*)>(.*?)</\1>", re.DOTALL)
COMPILE_REMOVE = re.compile(r'<Compile\s+Remove\s*=\s*"([^"]+)"')
PROJECT_REF = re.compile(r'<ProjectReference\s+Include\s*=\s*"([^"]+)"')


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
        if not path.is_file():
            continue
        chunks.append(path.read_text(encoding="utf-8"))
    return "\n".join(chunks)


def keyed_entries(rel_dir):
    folder = ROOT / rel_dir
    entries = {}
    if not folder.is_dir():
        errors.append("missing " + rel_dir)
        return entries
    for path in sorted(folder.rglob("*.xml")):
        text = re.sub(r"<!--.*?-->", "", path.read_text(encoding="utf-8"), flags=re.DOTALL)
        for match in KEYED_ENTRY.finditer(text):
            name = match.group(1)
            if name == "LanguageData":
                continue
            entries[name] = match.group(2)
    return entries


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
        include = raw.replace("\\", "/")
        if include.endswith("HungerAndHavoc.Api.csproj"):
            return True
    return False


def has_impl_project_reference(csproj):
    for raw in PROJECT_REF.findall(csproj):
        include = raw.replace("\\", "/")
        name = Path(include).name
        if name == "HungerAndHavoc.csproj":
            return True
    return False


def main():
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

    load = read("LoadFolders.xml")
    if "<li>Guard</li>" not in load or ">1.6</li>" not in load:
        errors.append("LoadFolders.xml must load Guard and 1.6")
    if "nanaloveyuki.mouse.disaster.famine.continued" not in load:
        errors.append("LoadFolders.xml must skip body when old mods are active")

    catalog = read("Source/Incidents/HungerIncidentCatalog.cs")
    ids = re.findall(r'"(I-\d{3})"', catalog)
    if len(ids) != 51:
        errors.append("catalog size %s, expected 51" % len(ids))
    if len(set(ids)) != len(ids):
        errors.append("duplicate display ids")
    expected = ["I-%03d" % i for i in range(1, 52)]
    if ids != expected:
        errors.append("display ids are not I-001..I-051 in order: %s" % ids[:8])
    defs = re.findall(
        r'(?:Original|Sequel)\(\s*"I-\d{3}",\s*"(RHAH_[A-Za-z0-9_]+)"',
        catalog,
    )
    if len(defs) != 51:
        errors.append("catalog def name count %s, expected 51" % len(defs))
    if "RHH_" in catalog:
        errors.append("catalog still has RHH_")

    zh_incidents = read("Languages/ChineseSimplified/Keyed/RHAH_Incidents.xml")
    en_incidents = read("Languages/English/Keyed/RHAH_Incidents.xml")
    for display_def in defs:
        key = "<RHAH_Incident_%s_Label>" % display_def[len("RHAH_"):]
        if key not in zh_incidents or key not in en_incidents:
            errors.append("missing label " + key)

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

    if not (ROOT / "Source/Api/HungerAndHavoc.Api.csproj").exists():
        errors.append("missing Source/Api/HungerAndHavoc.Api.csproj")
    else:
        api_csproj = read("Source/Api/HungerAndHavoc.Api.csproj")
        if has_impl_project_reference(api_csproj):
            errors.append("API csproj must not ProjectReference HungerAndHavoc.csproj")

    impl_csproj = read("Source/HungerAndHavoc.csproj")
    if impl_csproj:
        if not compile_remove_excludes_api(impl_csproj):
            errors.append(
                r"HungerAndHavoc.csproj must Compile Remove Api\**\*.cs"
            )
        if not has_api_project_reference(impl_csproj):
            errors.append(
                "HungerAndHavoc.csproj must ProjectReference HungerAndHavoc.Api.csproj"
            )

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

    comp_rel = "Source/Identity/CompHungerPawn.cs"
    comp_path = ROOT / comp_rel
    if not comp_path.exists():
        errors.append("missing " + comp_rel)
    else:
        comp = comp_path.read_text(encoding="utf-8")
        if not re.search(r"namespace\s+HungerAndHavoc\.Identity\b", comp):
            errors.append("CompHungerPawn must be in HungerAndHavoc.Identity")
        if re.search(r'"sourceIncidentId"', comp):
            errors.append("CompHungerPawn still uses save key sourceIncidentId")
        if not re.search(r'"sourceIncidentDisplayId"', comp):
            errors.append(
                "CompHungerPawn CompExposeData missing save key sourceIncidentDisplayId"
            )
        for token in COMP_SAVE_FIELDS:
            if token not in comp:
                errors.append("CompHungerPawn missing " + token)

    hediff = read("1.6/Defs/HediffDefs/RHAH_HungerMark.xml")
    if "RHAH_HungerMark" not in hediff:
        errors.append("Hunger mark missing defName RHAH_HungerMark")
    if "HungerAndHavoc.Identity.Hediff_HungerMark" not in hediff:
        errors.append(
            "Hunger mark hediffClass must be HungerAndHavoc.Identity.Hediff_HungerMark")
    genes = read("Biotech/Defs/GeneDefs/RHAH_Genes.xml")
    xenotypes = read("Biotech/Defs/GeneDefs/RHAH_Xenotypes.xml")
    if "RHAH_Gene_ThinRations" not in genes or "canGenerateInGeneSet>false" not in genes:
        errors.append("Owned hunger gene must stay out of random gene sets")
    if "RHAH_Xenotype_Ratkin" not in xenotypes:
        errors.append("Fallback xenotype RHAH_Xenotype_Ratkin is missing")
    if "HungerAndHavoc.Identity.CompProperties_HungerPawn" not in hediff:
        errors.append(
            "Hunger mark Comp Class must be HungerAndHavoc.Identity.CompProperties_HungerPawn"
        )

    skip_dirs = {".git", "tmp", "docs", "bin", "obj"}
    skip_files = {"NOTICE", "README.md", "Agents.md", "AGENTS.md", "verify-scaffold.py"}
    for path in ROOT.rglob("*"):
        if not path.is_file() or any(part in skip_dirs for part in path.parts):
            continue
        if path.name in skip_files or path.suffix.lower() not in {".cs", ".xml", ".md", ".ps1"}:
            continue
        text = path.read_text(encoding="utf-8", errors="ignore")
        rel = str(path.relative_to(ROOT))
        if "MouseDisaster" in text:
            errors.append("MouseDisaster leftover in " + rel)
        if re.search(r"\bRHH_", text):
            errors.append("RHH_ leftover in " + rel)
        if re.search(r"\b(?:RatkinEgg|IsEgg|HungerPawnRole\.Egg)\b", text):
            errors.append("ambiguous egg naming in " + rel)

    if not (ROOT / "docs/adr/0001-rewrite-design.md").exists():
        errors.append("missing adr 0001")
    if not (ROOT / "docs/adr/0002-pawn-mod-compatibility.md").exists():
        errors.append("missing adr 0002")
    if not (ROOT / "1.6/Assemblies").is_dir():
        errors.append("missing 1.6/Assemblies")

    if errors:
        print("FAIL")
        for item in errors:
            print(" -", item)
        return 1
    print("OK M0 scaffold: API assembly, Identity Comp, I-001..I-051, keyed symmetry")
    return 0


if __name__ == "__main__":
    sys.exit(main())
