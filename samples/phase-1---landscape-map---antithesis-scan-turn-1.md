All discovery and verification reads are complete. Here is the Phase 1 report.

---

# Phase 1 — Landscape Map & Antithesis Scan

**Scope:** Thesis-stage discovery for three candidate fields in multimodal/VLM research. **Window:** Jan 2025 → Sep 2026 (older items only as foundational). **Completeness:** 40+ arXiv items screened, ~10 read at abstract/full-text level; dataset/model/compute audits below. Statuses use the requested A–E scheme; every factual claim is tagged with its evidence type.

---

## 1. Executive Finding (TL;DR)

All three fields are **active, real, and already partially occupied**. The phenomena each hypothesis family wants to study have been *named and measured* in 2026:

- **Field A phenomena** have several published names already: *"text-only takeover"* / *"visual information encoded early, then image-token contribution drops"* (<https://arxiv.org/abs/2606.28864>), *"Look Light, Think Heavy"* visual-reflection decline (<https://arxiv.org/abs/2606.22565>), *"long-context visual forgetting"* (<https://arxiv.org/abs/2608.01314>), *"Visual Insensitivity Gap"* (<https://arxiv.org/abs/2609.00868>), *"Visual Relay Window"* three-stage attention redistribution (<https://arxiv.org/abs/2607.11436>), *"overshooting/attention in reasoning"* (<https://arxiv.org/abs/2501.19393>, text-only; <https://arxiv.org/abs/2606.28864> LVLM focus loss).
- **Field B phenomena** likewise: multimodal-vs-text ICL gap on matched counterfactual tasks (<https://arxiv.org/abs/2609.15028> = TwinICL), many-shot *"in-context collapse"* localized to the vision–language interface (<https://arxiv.org/abs/2608.02830>), *"surface imitation / copying most frequent label"* (<https://arxiv.org/abs/2609.10177>), implicit-M-ICL task vectors (<https://arxiv.org/abs/2608.13385>).
- **Field C phenomena** likewise: *"vision–operation misalignment"* with four named failure modes plus layer-wise causal attribution (<https://arxiv.org/abs/2607.16094>), *"Dense Same-Class Attribute Misbinding"* (DSCAM) benchmark (<https://arxiv.org/abs/2608.16805>), *commonsense prior override* of visual evidence (<https://arxiv.org/abs/2607.29240>, <https://arxiv.org/abs/2607.26326>).

**What is still open** is narrower than the motivating hypotheses: mechanism-vs-mechanism attribution (attention vs representation vs shortčcut), cross-modal-induction *veracity* (is it text-mediated?), and binding *locus* (where in the stack, and does instance identity survive). None of these is a "no one has touched this" topic; several are partially answered by the closest work, and the closest work is **fresh (Aug–Sep 2026)**, so novelty must rest on a sharply different measurement, not on the topic.

**Antithesis verdict:** no field is *dead* (all have verified, repeatable failure phenomena and open mechanistic sub-questions, see §V), but the naive version of each hypothesis is already occupied. Recommend Phase 2 = **YES** for exactly the 2–3 narrowly-scoped questions in §XXIII, each of which requires a result that no cited paper reports while every required public asset exists.

---

## 2. Landscape Comparison — Field A vs B vs C (truth table)

| Dimension | **A: Multimodal CoT dynamics & test-time compute** | **B: Cross-modal ICL & multimodal induction** | **C: Visual binding & compositional grounding** |
|---|---|---|---|
| Core phenomenon is real & measured | YES (4 independent measurements, 2026) | YES (2 independent, 2026) | YES (3 independent, 2026) |
| Phenomenon has a published name | YES — multiple competing names | YES — TwinICL gap / in-context collapse / surface imitation | YES — misalignment / misbinding / prior dominance |
| Independent replications | Partial (varied methods, small open models) | Partial (6 open models in TwinICL; varied panels elsewhere) | Partial (5 open + 2 API in DSCAM; 6 models in WhatIfVis) |
| Mechanistic (layer/causal) work done | Strong start (attention redistribution, activation patching, probing) | Weak (mostly behavioral; 1 causal lesion) | Strong start (causal layer interventions, steering vectors) |
| Dataset/benchmark assets (public) | Many, fresh (TwinICL-adjacent, WhatIfVis, Counter-SNLI-VE, GeoVAD) | TwinICL, MACRON/MMICL (foundational), CLBench-V | Winoground, SugarCrepe, VSR, MMVP, DSCAM/InstaBind-Lite, SPC-style CF sets |
| Small-open-model feasibility | HIGH (Qwen3-VL-2/4/8B, Gemma3n) | HIGH | HIGH |
| Crowding (2025–2026 papers found) | **HIGH** (>10 central) | **MEDIUM** (~6 central) | **MEDIUM-HIGH** (~8 central) |
| Verified unclaimed sub-question | Narrow (usage vs representation attribution along CoT) | Medium (is cross-modal induction text-mediated?) | Narrow (binding locus & instance-identity survival) |

---

## 3. Paper Map — Field A (CoT dynamics, test-time compute, visual utilization)

**Diagnosis / measurement (phenomenon established):**
1. <https://arxiv.org/abs/2606.28864> *On Test-Time Scaling for VLMs* (ECCV 2026) — 9 TTS methods, 6 benchmarks, multiple sizes; **"visual info encoded early in the reasoning chain, after which text-only reasoning dominates and image-token contribution drops"**; small models benefit most (up to ~30%); **LVLMs "lose focus when given more compute than necessary."** Direct pre-emption of the text-only-takeover hypothesis.
2. <https://arxiv.org/abs/2606.22565> *Look Light, Think Heavy* (ACL 2026) — 12 tasks, 14 non-reasoning + 8 reasoning models; **verbal reflection rises/falls while *visual reflection consistently diminishes***; CoT hurts perception tasks (visual grounding, counting); open reasoning-VLMs show only marginal overall gains (math-heavy overfitting). Direct pre-emption of "eye-heavy CoT will help broadly."
3. <https://arxiv.org/abs/2608.01314> *Remember-R1* (ACM MM 2026) — formalizes **long-context visual forgetting**; RL rewards for visual-keyword coverage + persistence of visual dependence in later steps; slows visual-attention decline. Intervention (training-side) already exists.
4. <https://arxiv.org/abs/2609.00868> *The Visual Insensitivity Gap* — **blurring the question-relevant region leaves next-token distribution ~unchanged on 40–97% of samples** (6 VLMs, 3 benchmarks); encoder linear-probe detects perturbation (0.72–0.79) while argmax changes only 2–11% (encoder→LLM gap >0.65); **sample-intrinsic** (VSI ranks correlate across models, ρ=+0.40). This is input-time utilization, not along-CoT — complements (1)–(3).
5. <https://arxiv.org/abs/2607.11436> *Ebb and Flow / Visual Relay Window* — mechanistic **three-stage depth-wise redistribution**: early question-conditioned org → middle visual-dominant relay → late answer formation; relay geometry causally tied to grounded generation; TRACE controls it at inference (+4.33 avg, up to +6.6 over 4 backbones, 7 benchmarks).
6. <https://arxiv.org/abs/2606.08464> TVI-CoT (vision-blind reasoning) — abstract-level match, mechanism partially overlapping.
7. <https://arxiv.org/abs/2609.06704> *vCT/vCCT counterfactual CoT faithfulness* (TU Wien) — CoTs don't track visual evidence that shifts predictions; Predict-then-Explain > pre-answer CoT; releases Counter-SNLI-VE, Counter-A-OKVQA (single-object edited pairs). Eval methodology directly reusable.
8. <https://arxiv.org/abs/2609.06746> *CVRR: latent visual reasoning made causally necessary* — shows latent image info can be informative yet unused causally.
9. <https://arxiv.org/abs/2606.13156> *Iterative Visual Thinking & the Self-Correction Mirage* — eval pitfalls in iterative refinement.
10. <https://arxiv.org/abs/2608.13760> *Amplified Does Not Mean Predictive* — claims about reasoning behaviors vs predictive utility (text-thinking models).

**Interventions / test-time compute for VLMs (occupied solutions):**
- Re-consultation: <https://arxiv.org/abs/2602.16702> SAP (saliency-aware multi-route; counters "visual input given once, then text-dominated; grounding errors accumulate"). <https://arxiv.org/abs/2604.11025> TTSP (grounding paradox; entropy-gated exploration + evidence ledger). Both are inference-time *methods*, not mechanism attribution.
- Latent/compressed visual thought: <https://arxiv.org/abs/2601.09536> Omni-R1, <https://arxiv.org/abs/2512.10941> Mull-Tokens, <https://arxiv.org/abs/2605.11856> UniVLR, <https://arxiv.org/abs/2607.28154> OPLD, <https://arxiv.org/abs/2609.21675> DRT, <https://arxiv.org/abs/2510.27492> ThinkMorph (interleaved text-image CoT, emergent text-scaling).
- Dynamic memory for visual evidence: <https://arxiv.org/abs/2609.05539> DLMR (visual memory + reasoning memory, router), <https://arxiv.org/abs/2608.29897> VERA (visual rendering as context manager; "modality-preserving" long-horizon context).
- Modality-attribution diagnostics: <https://arxiv.org/abs/2608.00076> Counterfactual Modality Attribution; <https://arxiv.org/abs/2609.06011> Tri-PvP (perceptual-propositional conflicts, layer-wise probing + contrastive decoding); <https://arxiv.org/abs/2609.00830> Visual Attention Faithfulness Heterogeneity.
- Training-side: <https://arxiv.org/abs/2606.01558> attention-guided fine-tuning improves CoT; <https://arxiv.org/abs/2609.04947> MCPO; <https://arxiv.org/abs/2608.26070> Prefix Sliding; <https://arxiv.org/abs/2608.21883> VIG (visual-information-gain reward).
- **Architecture-side modal competition (important framing finding):** <https://arxiv.org/abs/2609.00746> *Text Capability Loss in VL Adaptation* — VL fine-tuning corrupts the LLM's attention sink; a **Sink Strength** scalar on the base LLM predicts post-VL text-capability loss; QK-RMSNorm injection and weight merging fail to recover. This flips "modality competition" into a measurable, architecture-level quantity and is a direct confound for any A-thesis measuring attention redistribution.
- Runtime/tokens: <https://arxiv.org/abs/2609.01004> SinkPruner (high-norm visual outlier tokens = attention-sink artifacts), <https://arxiv.org/abs/2609.00667> RaDiCal (visual-token pruning for rerankers), <https://arxiv.org/abs/2608.15962> SEER, <https://arxiv.org/abs/2608.08630> VLZip.

**Test-time compute foundations:** <https://arxiv.org/abs/2501.02497> TTS survey (o1-era); <https://arxiv.org/abs/2501.19393> s1 (budget-forcing, "Wait" — text-only); <https://arxiv.org/abs/2606.08231> (TTS variants survey). Multimodal o1-style generalization is explicitly study #1's subject.

## 4. Paper Map — Field B (multimodal ICL & cross-modal induction)
1. <https://arxiv.org/abs/2609.15028> **TwinICL** — procedurally generated **matched text/image task pairs**; 6 open models, 38 tasks; multimodal ICL < text-only ICL everywhere, gap varies by task family; three interventions (visual access, task framing, reasoning) recover combined but not individually; **modality gap persists even with explicit task instructions** → separates *inference* vs *execution* of the task; demonstrations act as "context to process + evidence about the task." (github.com/lab-flair/TwinICL). Fresh (14 Sep 2026) — direct overlap risk: this is the exact "do images matter" question already benchmarked.
2. <https://arxiv.org/abs/2608.02830> **In-Context Collapse (CircA)** (Amazon AGI) — many-shot image–label ICL **collapses** (some models below chance) while outputs stay well-formed; dissociates (i) robustness to accumulating demos from (ii) learning a novel rule in context → three regimes; **lesion/parà-rescue localizes collapse to connector + early/mid layers** ("integration pathway"), not late readout; one-time "vaccine" adapter transfers cross-task. This is the causal scaffold a B-thesis would need, already built.
3. <https://arxiv.org/abs/2609.10177> *Beyond Surface Imitation* (SYSU) — MLLMs in multimodal ICL copy the **most frequent textual label** when demos share global visual features; proposes contrastive demonstration reformulation. Names the shortcut.
4. <https://arxiv.org/abs/2608.13385> *When Is a Task Vector Enough?* — Selection–Realization Hypothesis for **implicit multimodal ICL**; static task vector vs query-conditioned interventions; task vector success tied to how much demo-induced change is shared across queries.
5. <https://arxiv.org/abs/2608.12724> *MAG* — semi-supervised demo selection for MLLM ICL; **textual representations more effective for relevance propagation, visual+textual crucial for demo selection** (modality-asymmetric retrieval). Directly relevant: implies induction leverage is text-driven.
6. <https://arxiv.org/abs/2609.15683> V-ICAL (video ICL for agents) and <https://arxiv.org/abs/2608.24119> TransPhy/PhysVICL-74 (visual ICL for image editing) — adjacent VICL generation/agent settings.
7. <https://arxiv.org/abs/2607.07117> ToT-T2I-ICL (compositional pattern inference for T2I); <https://arxiv.org/abs/2609.10613> ICL jailbreaks as posterior reweighting (MLLM safety lens; demos as evidence — useful framework, peripheral topic).
8. Foundational (pre-2025, not re-scored): Flamingo (image-conditioned ICL), MMICL (ICCV'23, coarse-to-fine alignment for ICL), MACRON (ICLR'24, multimodal analogical retrieval) — the "induction as retrieval" framing these introduced is the lineage this field builds on.
9. <https://arxiv.org/abs/2608.27417> *Retrieval Heads Meet Vision* — retrieval-head-style analyses extended to MLLMs (abstract-level; not full-text verified, search-result status).

**Crowding read on B:** fewer papers than A, but the two most important questions (multimodal-vs-text gap; collapse + layer localization; surface imitation) are occupied; demo-order/label-permutation specifically in the **multimodal** setting was **NOT FOUND** in the 2025–2026 window (only text-ICL and demo-selection papers), and the caption-substitution test (replace image demos with their textual descriptions and ask whether the ICL advantage/loss moves) was **NOT FOUND** — see §XXIII.

## 5. Paper Map — Field C (binding & compositional grounding)
1. <https://arxiv.org/abs/2607.16094> **How Do VLMs Fail? Vision–Operation Misalignment** (ACM MM 2026) — operation-centric mechanistic decomposition of compositional VQA failures into **grounding / reasoning / attribute-extraction / language-prior-dominance**; causal interventions across all layers: object-selection→feedforward, multi-step relational→**late-layer direct attention**, attribute-extraction→**answer-position FF**; VSR validation (single-step spatial failures at object-position encoding). This is the closest single paper to a binding-locus thesis.
2. <https://arxiv.org/abs/2608.16805> **DSCAM / InstaBind-Lite** — dense same-class attribute misbinding: 524 images, 529 groups (3–6 same-class entities), 1773 boxed instances, 9580 deterministic questions; open models 19.84% misbinding, API 7.55%; **80.7% of transfers originate from adjacent instances**; localization & instance-first interventions help only some models. Turns wrong answers into source-identifiable categories.
3. <https://arxiv.org/abs/2607.26326> **Seeing or Knowing?** (WhatIfVis) — coarse-grained visual evidence (color/count/size/weight/spatial) is *reconstructible from final-layer image tokens* → failures are **post-perceptual utilization**, not encoding; visual-context sensitivity is unstable; **activation patching localizes vision-vs-prior trade-off at architecture-specific depths**; a steering vector controls the trade-off. Directly locates "language prior override" as a mid-stack, steerable quantity.
4. <https://arxiv.org/abs/2607.29240> *SPC* — commonsense-driven hallucination: model's prior overrides *visible counterfactual* evidence (e.g., six-fingered hand → 5); errors coincide with candidates preferred image-free; images answered wrong on CF sets match image-free preference → predicted by prior, repairable by selective prior calibration. Quantitative predecessor of a "language prior" thesis.
5. <https://arxiv.org/abs/2606.29462> *MIRROR* — MLLM projection alignment "trains each visual token to carry the right semantics but never whether *relations between concepts* survive the crossing from language to vision"; geometric (Gromov–Wasserstein) supervision to transfer relational priors. The *relational-structure bridge* framing of binding.
6. CLIP-generation compositionality: <https://arxiv.org/abs/2609.08242> CS-CLIP (element-specific biases vs vanilla CLIP), <https://arxiv.org/abs/2608.25575> MLLMCLIP (feature-level MLLM→CLIP distillation), <https://arxiv.org/abs/2609.04083> CORE (attribute-object binding failures in MLLM embeddings, COLA/SugarCrepe++/NegBench), <https://arxiv.org/abs/2606.26794> ReasonCLIP-58M, classic <https://arxiv.org/abs/2504.04740> SCRAMBLe (Winoground-based compositional preference tuning, 2025).
7. Shortcut/confound sources for C: <https://arxiv.org/abs/2609.16567> VQA-CP-v2 counterfactual training (language-bias shortcut), <https://arxiv.org/abs/2608.30480> VisER (separating visual evidence from generated-prefix support in hallucination detection), <https://arxiv.org/abs/2608.09772> PragMatch (surface-cue sensitivity in LVLMs).
8. Adjacent spatial/compositional stress tests (not binding per se): <https://arxiv.org/abs/2608.10864> MVRD, <https://arxiv.org/abs/2609.16233> SceneBench, <https://arxiv.org/abs/2608.26716> CoDeLayout, <https://arxiv.org/abs/2609.06880> POVBench, <https://arxiv.org/abs/2608.21832> GUI-Primitives, <https://arxiv.org/abs/2608.20414> StateSight, <https://arxiv.org/abs/2607.12786> CoRe-Bench.
9. **Dynamic binding** (binding-problem lineage: object files, feature binding across instances, role-filler) in MLLM representations: **NOT FOUND** in window (searches surfaced CLIP-era Winoground work and fingerprinting/agents). If a thesis wants the strongest "missing thing," this is where the terminology-substitution check is cleanest — but beware it may be *not studied* for lack of tractable instrumentation rather than for lack of interest.

---

## 6. Antithesis Findings (status + confidence per core hypothesis)

**H-A1 — "VLMs under-attend to visual tokens during long CoT (gradual text-only takeover)."**
- STATUS B (supported by independent measurements). 2606.28864 (token contribution drops along chain), 2606.22565 (visual reflection diminishes), 2608.01314 (visual forgetting; visual attention decline), 2607.11436 (3-stage redistribution; late return to answer formation away from visual relay), 2609.00868 (input-time insensitivity). **But** 2609.00746 shows an architecture-level confound (sink corruption during VL fine-tuning) and 2609.00830 shows faithfulness heterogeneity both across heads and across models. Confidence: **HIGH** that the *phenomenon* exists; **MEDIUM** that any single mechanism (attention vs representation vs shortcut) is the cause — that attribution is not yet closed, which is the only live opening.
- Waiver/hedge: any "we show VLMs ignore images during reasoning" thesis is already-written unless the *measurement* (per-position, causal, consistency across the 3 claimed phases) is new and reconciles 2606.28864 vs 2609.00868 (early-snapshot vs input-position insensitivity).

**H-A2 — "Test-time compute allocation between visual processing and textual reasoning is a tunable axis."**
- STATUS B, method-spaces crowded: SAP (2602.16702), TTSP (2604.11025), TRACE (2607.11436), OPLD/DRT/UniVLR/Mull-Tokens/Omni-R1 (latent visual thought), DLMR (2609.05539), VIG (2608.21883). Plus 2606.28864 finding that *small* models benefit most and models "lose focus with extra compute." Confidence: **HIGH** the axis exists; **MEDIUM-HIGH** the marginal contribution of yet another intervention is small. Negative-result angle: no paper yet cleanly reports the *compute-for-visual-inspection vs compute-for-textual-reasoning* ROI curve with matched budgets; 2606.28864's "lose focus" is suggestive but not a satisfying curve. Status overall: **B with diminishing-marginal-research risk**.

**H-B1 — "Multimodal ICL underperforms, and image demos contribute little beyond textual shortcuts."**
- STATUS A (directly investigated, with nuanced result): TwinICL (2609.15028) — weakness confirmed, but *modality gap persists when task is known* (→ not only demo induction; execution also matters), and combined interventions recover a diagnostic subset → "images don't matter" is **falsified** as stated; the correct picture is modality-asymmetric induction + execution gap. Surface imitation (2609.10177) and text-driven relevance (2608.12724) support shortcutism for *demo selection* but not for *rule induction*. Confidence: **HIGH**. Any thesis claiming "multimodal ICL ignores images" as novel is dead; the live version is the caption-substitution equivalence test (§XXIII).

**H-B2 — "Demonstration order/permutation acts differently across modalities / drives cross-modal induction."**
- STATUS D/UNDERDETERMINED. Ordering/label-permutation sensitivity in **multimodal** ICL: **NOT FOUND** in window (only text-ICL and demo-selection literature, e.g., 2609.17888, 2609.10177). No evidence of c>claim-by-others, no evidence against. Confidence: **LOW** on both directions — genuinely open but thin; risk that the effect is small/empty (negative result likely, which is itself publishable only if benchmarked properly).

**H-C1 — "Attribute/relation binding to the correct visual instance is fragile in MLLMs (misbinding/prior override)."**
- STATUS A (verified as a phenomenon): DSCAM (2608.16805) at 19.8% misbinding on open models with adjacent-instance dominance; SPC (2607.29240) prior-override; WhatIfVis (2607.26326) post-perceptual utilization failure; Winoground-level failures replicated in MLLM era (2504.04740). Confidence: **HIGH** phenomenon, **HIGH** that it is not caused by encoding loss for coarse attributes (WhatIfVis reconstruction).

**H-C2 — "The binding failure has a specific layer/attention locus, and instance identity is lost or confounded mid-stack."**
- STATUS C (partially examined): 2607.16094 attributes *attribute-extraction* failures to answer-position FF and *relational* failures to late-layer direct attention; 2607.26326 localizes the vision-vs-prior trade-off to architecture-specific depths via activation patching; 2606.29462 argues the *projection* never enforces cross-token relational structure. But **same-class instance-specific binding across the depth axis** (per what DSCAM measures) has **no** layer-locus study found. Confidence: **MEDIUM** that the question is answerable; **MEDIUM** it is unclaimed. This is the strongest Field C opening.

---

## 7. Dataset & Benchmark Audit (licenses, access, leakage, saturation)

| Benchmark | Access / license | Size | Saturation / caveats |
|---|---|---|---|
| *Winoground* (CVPR'22) | HF **gated** (research-use + conditions; 733 MB); COL J. per-paper | 400 cases, 2×2 | Group-scoring is gameable (2510.07632 shows simple matching beats CLIP scores); CLIP-era; needs careful protocol |
| *SugarCrepe* (NeurIPS'23) | **MIT**, GitHub + COCO-val images; HF mirror (MIT) | >1k | All models struggle on SWAP subset; object-centric; 1×k metric safe |
| *VSR* / *ARO / Cola / VL-CheckList* | MIT / per-repo | – | Classic compositional stress sets; ARO/Cola have known hackability (fix by SugarCrepe) |
| *MMVP, MMStar, MMMU* | MMVP (2024) public; MMStar public; MMMU multi-license per source | – | Aggregate suites; VSI shows aggregate accuracy hides insensitivity (2609.00868); contamination taxonomy exists (2608.29463) — disclose split/employment |
| *TwinICL* (2609.15028) | public (github.com/lab-flair/TwinICL), procedurally generated | 38 tasks, matched pairs | Fresh Sep-2026; **matched text/image counterfactual guarantee** = best asset for Phase-2 B-q |
| *InstaBind-Lite/DSCAM* (2608.16805) | benchmark presented with deterministic auto-grading | 9580 questions / 524 imgs | Fresh Aug-2026; source-instance annotations separate hallucination vs recognition vs binding — ideal for Phase-2 C-q |
| *WhatIfVis* (2607.26326) | introduced in paper (5 dims × image-vs-prior questions) | – | Best for prior-vs-visual controllability reuse |
| *Counter-SNLI-VE, Counter-A-OKVQA* (2609.06704) | released by authors | single-object-edited pairs | Reuse for CoT-faithfulness controls |
| *GeoVAD-Bench* (2609.12606), *CLBench-V* (2607.25294), *CoRe-20K/Bench* (2607.12786), *POVBench* (2609.06880), *SceneBench* (2609.16233), *M3R-Bench* (2608.05817), *GUI-Primitives* (2608.21832), *StateSight* (2608.20414), *MUSE* (2609.19088), *MultivationBench* (2607.26465), *EDCT-Bench* (2609.17953), *VGAU-Diag* (2608.22174), *PhysVICL-74* (2608.24119), *MUSE-Bench* (2609.15087) | per-paper | Mostly fresh 2026; spatial/relational stress tests; most beyond thesis scope but Usable as transfer/robustness evals |
| *MMSD2.0-derived PragMatch* (2608.09772) | 3,000 contrastive pairs | sarcasm-only | Peripheral |

**Audit conclusions:** every Phase-2 asset needed exists, is public or near-public (Winoground gating is a minor friction), is fresh, and has low contamination risk (2026 procedurally-generated suites). Visual-token/hidden-state instrumentation (needed for mechanism work) is *not* a benchmark problem — it is a library/engineering problem (below).

## 8. Model & Compute Audit (feasibility on Colab/cloud GPU)

**Candidate panels (all open weights, HF-style), verified against official/tech-report sources:**
- *Qwen3-VL* 2B / 4B / 8B / 32B dense + 30B-A3B / 235B-A22B MoE, **Instruct and Thinking variants** (≥2B released 2025-10), 256K ctx, Apache-2.0 tech report <https://arxiv.org/pdf/2511.21631>. 4B-VL ≈ 10 GB VRAM FP16 inference (Spheron numbers). This family is *already the standard* in the 2026 mechanics literature (Ebb&Flow, On-TTS, Circular, etc. run it).
- *Gemma 3 4B* (multimodal, 128K, QAT) ≈ 3.7 GB recommended (canirun.ai); Gemma 3n E2B; Gemma-4 family (2/4B edge, 12B unified) per Google AI docs. Gemma license = usable for research.
- *Qwen2.5-VL-7B*, *LLaVA-1.5/OneVision-7B*, *InternVL2-4B*, *SmolVLM2* — all used in the closest-work papers; LLaVA and SmolVLM are the lightest for quick attention/probe experiments.
- Context note: Qwen3-4B-class models run on an **8 GB** GPU at Q4 (VRAM guide 2026); FP16 4B-VL needs ~10–16 GB → T4-16GB/A10G on Colab Pro or equivalent cloud is sufficient for 2B/4B; 8B needs ~16–20 GB (A100/4090).

**Instrumentation feasibility (the real compute risk):**
- Attention/activation extraction (needed to measure "attention vs representation vs shortcut") uses standard HF hooks — this is exactly what Ebb&Flow/TRACE, WhatIfVis activation patching, vCT/CCT, and CVRR did on these model families. So feasible, but **non-trivial** for Qwen3-VL (no public attention-hook recipes beyond text-only `output_attentions` paths); LLaVA/SmolVLM are friendlier for the ablation end.
- "Thinking" variants (reasoning traces) increase token budget and KV-cache; 256K ctx advertised but practical runs for a thesis should cap at ~8–16K tokens/sample on consumer GPUs; Q8 KV caching cuts memory (VRAM guides 2026).
- No frontier-pretraining requirement anywhere in the shortlist; all experiments are inference/RL-lite/eval-scale → **fully compatible with the stated constraints** (public data, reproducible baselines, Colab/cloud class GPUs).

## 9. Shortcut & Confound Audit (what would fake each hypothesis)

1. **Text contamination of "visual reasoning":** questions posed in text already carry the answer's priors (MMMU/MMStar style) → any VSI/blur/probe result must hold after caption- and prior-only baselines (WhatIfVis's design is the model to copy).
2. **Attention-sink artifacts:** high-norm visual outlier tokens and sink corruption (2609.01004, 2609.00746) can inflate "attends to image" and deflate "text capability" readings; filter outlier tokens before any attention claim.
3. **Sum/rank vs actual usage:** aggregate benchmark accuracy is known to hide insensitivity (2609.00868); Pop=score/vocab stats can be driven by a handful of tokens; use next-token-distribution deltas and per-sample metrics.
4. **Metric gaming in group-structured sets:** Winoground-style GroupScore is hackable (2510.07632); use 1×k tasks or matched-pair protocols.
5. **Order/length confounds in ICL collapse:** accumulated demos lengthen context; separate "robustness to length" from "rule learning" — CircA explicitly shows these are dissociable capacities (three regimes) → must control both axes.
6. **Demo modality ≠ rule modality:** in any B-experiment the image content must be *causally* varied (swap/caption it) to prove induction is image-mediated, not just co-occurring.
7. **Dataset leakage/split hygiene:** use the new procedural benchmarks (TwinICL, InstaBind-Lite) + report contamination per 2608.29463 five-type taxonomy.
8. **Prior-override vs perception failure:** always include the image-free/prior-only control (WhatIfVis 3-condition; SPC's image-free preference protocol) before attributing a failure to "binding."

## 10. Negative-Result Value Analysis (would "nothing found" still publish?)

- A: a *rigorous* null/negative — e.g., "re-attending visual tokens late in CoT does **not** restore answer correctness once text-only reasoning has begun" — would refine the frontier: it contradicts the implicit assumption of re-consultation methods (SAP/TTSP/TRACE) and would be a real scientific contribution, provided it reconciles with 2606.28864's and 2609.00868's measures. MED-LOW risk because the field currently over-assumes re-consultation helps.
- B: "multimodal ICL gains vanish when image demos are replaced by their captions" (informational-substitution negative) is a clean, cheap falsifiability result regardless of sign — if TRUE, kills "genuine cross-modal induction" claims; if FALSE, motivates why images add something. High value, low cost.
- C: "same-class attribute misbinding survives all demonstration/instruction interventions" — DSCAM already shows interventions are partial; a negative on *layer-steering* would close the C-locus approach cleanly. MED value.

## 11. Closest-Work Matrix (must-be-beaten / must-differ-from)

| Thesis idea (blocked) | Closest work | What it already did | What remains (your delta) |
|---|---|---|---|
| "VLMs text take over during CoT" | On TTS for VLMs (2606.28864); Look Light (2606.22565) | Measured token-contribution drop/shape over time | Per-position causal attribution of the drop to attention vs representation, reconciled on the same model panel |
| "visual attention decays in long CoT" | Remember-R1 (2608.01314); Ebb&Flow (2607.11436) | RL mitigation; 3-stage relay + inference control | Distinguishing attentive-unable from able-inattentive along the trajectory (VSI was input-time) |
| "images get ignored by MLLMs" | VSI (2609.00868) | Blur-probe insensitivity + encoder-decoder gap | Along-CoT/step-level sensitivity; does CoT *position* modulate the gap? |
| "multimodal ICL < text ICL" | TwinICL (2609.15028) | Matched pairs; inference-vs-execution gap; interventions | Caption-substitution equivalence + demo-order permutations on the *same* 6-model panel |
| "M-ICL collapse lives at the interface" | CircA (2608.02830) | Lesion localizes connector+early/mid layers; transferable adapter | Mechanistic induction-vs-copy attribution (which heads implement the rule?) |
| "M-ICL is label-copying" | Surface Imitation (2609.10177) | Named most-frequent-label copying; contrastive fix | Order/permutation + content-swap manipulation isolating *when* copying is used vs genuine rule application |
| "binding failures in MLLMs" | DSCAM (2608.16805) | Behavior-level misbinding + interventions | **Layer locus of binding** (activation patching at binding-relevant positions) |
| "language prior dominates vision" | SPC (2607.29240); WhatIfVis (2607.26326) | Prior-override predicted by image-free preference; steerable, depth-specific trade-off | Crossing this confound with *same-class* binding (instance identity, not just attribute values) |
| "compositional VQA failures decompose mechanistically" | Vision–Operation Misalignment (2607.16094) | 4 failure modes; FF vs attention attribution | Extension to same-class binding across depths — their analysis covered VQA ops, not instance-level identity |

Rule applied throughout: no candidate proceeds unless its *measurement* differs, since the closest work is 2026-fresh.

## 12. Research Crowding Map (who publishes where)

**Cluster 1 — EU interpretability (Brussels/TU-Wien/Copenhagen):** On TTS-VLMs (VUB, <https://arxiv.org/abs/2606.28864>); vCT/CCT CoT-faithfulness (TU Wien, <https://arxiv.org/abs/2609.06704>); Seeing/Knowing WhatIfVis (U. Copenhagen/Cornell, <https://arxiv.org/abs/2607.26326>). — Active on A-mechanics.
**Cluster 2 — China causal-CoT (ICT/UCAS/HIT/SEU-UESTC style):** Look Light Think Heavy (CAS-ICT, <https://arxiv.org/abs/2606.22565>); Remember-R1 (HKUST/HKU-linked, <https://arxiv.org/abs/2608.01314>); Ebb&Flow group (UESTC et al., <https://arxiv.org/abs/2607.11436>); SPC (HIT-SZ, <https://arxiv.org/abs/2607.29240>); DLMR (NJU-affiliated, <https://arxiv.org/abs/2609.05539>). — A & C activity highest here.
**Cluster 3 — Korea:** Sink Strength (Korea Univ + NAVER, <https://arxiv.org/abs/2609.00746>); MVRD (KAIST, <https://arxiv.org/abs/2608.10864>); CS-CLIP (SNU, <https://arxiv.org/abs/2609.08242>). — A-architecture & C-benchmarks.
**Cluster 4 — M-ICL specialists (US-TW cluster):** TwinICL (UCLA/NTU/TAMU/Emory-based, <https://arxiv.org/abs/2609.15028>); In-Context Collapse (Amazon AGI, <https://arxiv.org/abs/2608.02830>); MAG + implicit-M-ICL (varied, <https://arxiv.org/abs/2608.12724>, <https://arxiv.org/abs/2608.13385>). — B is a *small* OG cluster (~5 groups) — least crowded by people count.
**Cluster 5 — vision-centric C engineers:** InstaBind-Lite (independent/Northeastern-linked, <https://arxiv.org/abs/2608.16805>); How Do VLMs Fail (BYU-affiliated + industrial, <https://arxiv.org/abs/2607.16094>); MIRROR (<https://arxiv.org/abs/2606.29462>); VisER (Melbourne, <https://arxiv.org/abs/2608.30480>). — C mechanistic works emerge from *isolated* groups, not an organized field → lower coordination/turnover than A, but fast.

**Implication:** any A-thesis competes with ~5 active groups publishing monthly; B competes with ~2–3 groups (but one owns the benchmark); C's mechanism sub-niche has <3 groups.

## 13. Candidate Question Inventory (3–5 per field; NOT recommendations)

**Field A:**
1. Along a multimodal CoT, is the drop in image-token contribution (2606.28864) caused by *reduced attention mass* on image tokens, *decayed/irrelevant hidden states*, or *output-distribution anchoring to text prefixes* — measured per step with matched counterfactual edits? (mechanism, fills a real gap).
2. Does *late-CoT re-attending* to the image restore accuracy when the model has committed to a text-only trajectory (falsifies/refines SAP/TTSP re-consultation assumption)?
3. Does CoT position modulate the VSI encoder→LLM gap (2609.00868), i.e., is input-time insensitivity consistent with step-level insensitivity?
4. For overloaded compute, where exactly does "losing focus with more compute" occur (which phase flips to text dominance) and is it architecture-specific (sink-health, 2609.00746)?
5. Do existing TTS methods for LVLMs help *visual* reasoning tasks specifically, or only text-heavy ones (Look-Light vs Think-Heavy dichotomy, 2606.22565)?

**Field B:**
1. Is "genuine" cross-modal induction an illusion: does caption-substitution of image demos preserve/eliminate the multimodal-ICL advantage, and does the answer vary by demo modality, model, and task family? (informational-substitution test — the key live B question).
2. Does demo *order / label permutation* sensitivity differ between text-only and multimodal demonstrations on matched tasks (TwinICL-style)?
3. Which mechanism implements the "collapse-resistance vaccine" of CircA (induction heads vs copy heads) when measured with image-token attention directly?
4. When the task is *already known* (instruction given), what remains of the multimodal gap — i.e., is the residual deficit in *execution on the query image*, not demo inference (TwinICL's open end)?
5. Is surface imitation (label-copying) modulated by visual similarity in a *causal* way (swap demo images and track label-copy rate)?

**Field C:**
1. Where in the transformer does same-class attribute binding collapse: does activation patching at object-position-encoding vs attribute-position-encoding break/repair DSCAM-type bindings (extending 2607.16094's method to instance binding)?
2. Does instance identity survive into late layers (probe correspondence between visual token identity and semantic role), or is the misimage class-level, not instance-level?
3. Is attribute binding to same-class instances dominated by *language priors about typical attributes* (SPC protocol, 2607.29240) or by *perceptual adjacency* (DSCAM, 2608.16805) — and how does the prior/percept ratio change across depths?
4. Does steering the WhatIfVis vision-vs-prior vector (2607.26326) transfer to *binding* errors, i.e., is misbinding a utilization failure, not an encoding failure?
5. Do relational structures survive the projection (MIRROR's claim, 2606.29462) — testable with a projector-randomization experiment on 1–2 small MLLMs?

## 14. Candidate Question Shortlist — Selection Logic

Criteria applied: (i) real phenomenon, (ii) exact question not already answered by any window paper, (iii) terminology-substitution done, (iv) small-open-model experiment, (v) public eval assets, (vi) ≥1 strong falsifiable alternative, (vii) useful-negative. After intersection with the audit:

- **B-1** passes all seven (the only fully open question with a clean falsifiable design on a released benchmark).
- **C-1** passes all seven (extends an existing method to an *unmeasured object* — instance binding — on a released benchmark).
- **A-1** passes (iii)-(vii) but fails (ii) *partially*: contributors overlap with 2606.28864/2609.00868/2608.01314; the delta is attribution only. → kept as the third option because it is the highest-value to the field, with explicit risk flagged.
- Candidates 2/3/4/5 in each field were eliminated for method-space crowding (A2, A4, A5), benchmark ownership by a single group (B2/B4 interlock with TwinICL's open ends), or insufficient falsifiability leverage (C3, C4).

## 15. Phase-1 Conclusion — Status & Phase-2 Recommendation

**Field A — Status: PRESENT & CROWDED.** Verified phenomena (text-only takeover, visual forgetting, Look-Light/Think-Heavy, VSI), verified interventions (remembering, relay-window control, re-consultation, latent visual thought, TTS), verified eval pitfalls. **Phase 2 for A: UNCERTAIN** — only the usage-vs-representation attribution (A-1) survives the antithesis, and it is a mechanism-measurement project on a fast-moving field.

**Field B — Status: PRESENT, SMALLEST FIELD, ONE OWNER.** The gap is verified; the exact mechanism (text-mediated vs image-mediated induction) is not, and no caption-substitution or multimodal-specific order/permutation study exists in-window. **Phase 2 for B: YES** (via B-1) — cheapest, most falsifiable, healthiest negative-result profile, benchmark already released.

**Field C — Status: PRESENT, PARTIALLY CROWDED AT BEHAVIOR LAYER, UNDEFENDED AT BINDING-LOCUS.** Misbinding, prior-override, post-perceptual utilization all verified; layer-locus of *same-class instance binding* is unmeasured. **Phase 2 for C: YES** (via C-1) — releases a measurable object (binding locus/instance-identity survival) no cited paper reports, on a deterministic benchmark with every intervention tool already demonstrated by 2607.16094 and 2607.26326.

**Overall Phase 2 = YES — with 2 candidate questions that must be sharpened into exact, pre-registered experiment briefs (B-1 and C-1 captured below for the record; A-1 held as reserve).** No field is dead; none is safe.

*This closes Phase 1. Phase 2 (design, baseline calibration, and experiment execution) has not been started.*

---

## 16. Evidence Table (summary of supporting sources)

| Claim | Best source(s) | Evidence type |
|---|---|---|
| Visual info encoded early, text dominance later; small models gain most; extra compute hurts | <https://arxiv.org/abs/2606.28864> | Direct paper evidence (read abstract/full-text metadata; ECCV'26) |
| Verbal reflection rises, visual reflection monotonically decays; CoT hurts perception tasks | <https://arxiv.org/abs/2606.22565> | Direct paper evidence (ACL'26, read abs + affiliation) |
| Long-context visual forgetting exists; RL slows visual-attention decline | <https://arxiv.org/abs/2608.01314> | Direct paper evidence (ACM MM'26, read) |
| Input-time insensitivity (VSI) across 6 VLMs; encoder-decoder gap; sample-intrinsic | <https://arxiv.org/abs/2609.00868> | Direct paper evidence (read) |
| Three-stage attention redistribution; VRW causal to grounding; TRACE control | <https://arxiv.org/abs/2607.11436> | Direct paper evidence (read) |
| MLLM→text text-capability loss via attention-sink; Sink Strength predictor | <https://arxiv.org/abs/2609.00746> | Direct paper evidence (abstract+credible design) — **unverified** beyond abstract |
| Multimodal ICL < text ICL on 38 matched tasks/6 models; gap persists with instructions | <https://arxiv.org/abs/2609.15028> | Direct paper evidence (read; dataset released) |
| Many-shot in-context collapse; connector/early-mid layers causal; transferable vaccine | <https://arxiv.org/abs/2608.02830> | Direct paper evidence (abstract) — mechanism claims unverified at full-text |
| M-ICL demo selection is text-driven; images matter for final selection | <https://arxiv.org/abs/2608.12724> | Search-result + abstract — **unverified** |
| Surface imitation/most-frequent-label copying | <https://arxiv.org/abs/2609.10177> | Abstract evidence — full detail unread |
| Attribute misbinding (DSCAM), adjacency dominance, partial interventions | <https://arxiv.org/abs/2608.16805> | Direct paper evidence (abstract) + dataset spec |
| Four failure modes; FF vs attention layer attribution; VSR validation | <https://arxiv.org/abs/2607.16094> | Direct paper evidence (read; ACM MM'26) |
| Reconstruction proves encoding OK; activation-patch localization; steerable prior trade-off | <https://arxiv.org/abs/2607.26326> | Direct paper evidence (abstract; cross-paper inference for method-details) |
| Commonsense prior override predictable from image-free preference | <https://arxiv.org/abs/2607.29240> | Abstract evidence — full detail unread |
| Projection fails to transfer inter-concept relations | <https://arxiv.org/abs/2606.29462> | Abstract evidence — mechanistics unverified |
| Compositionality benchmarks gating/licenses (Winoground gated/MIT; SugarCrepe MIT; hackability) | <https://huggingface.co/datasets/facebook/winoground>; <https://github.com/RAIVNLab/sugar-crepe>; <https://arxiv.org/abs/2510.07632> | Dataset documentation / official repo / cross-paper |
| Model availability/VRAM (Qwen3-VL sizes+licensing; Gemma3 4B; 8GB-class feasibility) | <https://github.com/QwenLM/Qwen3-VL>; <https://ai.google.dev/gemma/docs/core>; canirun.ai & Spheron guides | Official repo / cross-source compute guides |
| Contamination taxonomy (five types; disclosure protocol) | <https://arxiv.org/abs/2608.29463> | Direct paper evidence (abstract) |
| No multimodal ICL order/permutation study; no caption-substitution test; no dynamic-binding-in-MLLM work found | negative claims: `orx discover` over window | Search result + NOT FOUND |

## 17. Source Links
All arXiv links cited inline above (IDs: 2606.28864, 2606.22565, 2608.01314, 2609.00868, 2607.11436, 2609.00746, 2609.15028, 2608.02830, 2608.12724, 2609.10177, 2608.16805, 2607.16094, 2607.26326, 2607.29240, 2606.29462, 2609.06704, 2609.06746, 2606.13156, 2608.13760, 2602.16702, 2604.11025, 2601.09536, 2512.10941, 2605.11856, 2607.28154, 2609.21675, 2510.27492, 2609.05539, 2608.29897, 2608.00076, 2609.06011, 2609.00830, 2606.01558, 2609.04947, 2608.26070, 2608.21883, 2609.01004, 2609.00667, 2608.15962, 2608.08630, 2501.02497, 2501.19393, 2606.08231, 2608.13385, 2609.15683, 2608.24119, 2607.07117, 2609.10613, 2609.16233, 2608.26716, 2609.04083, 2608.25575, 2606.26794, 2504.04740, 2609.16567, 2608.30480, 2608.09772, 2608.10864, 2609.06880, 2608.21832, 2608.20414, 2607.12786, 2609.12606, 2607.25294, 2609.21246, 2608.29463, 2609.17888, 2510.07632, 2511.21631, 2608.27417). Dataset/model assets: github.com/lab-flair/TwinICL, github.com/RAIVNLab/sugar-crepe, huggingface.co/datasets/facebook/winoground, github.com/QwenLM/Qwen3-VL, ai.google.dev/gemma.

---

**Phase-2 shortlist (recorded for next phase, not started):**

1. **B-1 — "Is cross-modal induction text-mediated?"** Replace image demonstrations with their textual descriptions (informational-substitution) across TwinICL's paired task families and the six open models; measure whether the multimodal-ICL deficit and its interventions survive caption-substitution; falsifiable enemies: (i) images carry irreducible information (advantage survives), (ii) captions ≥ images (induction is text-mediated), (iii) mod-epo equal (induction is neither), plus prior-only and copy-rate controls.
2. **C-1 — "Where does same-class attribute binding fail?"** On InstaBind-Lite samples, apply 2607.16094-style causal layer interventions (FF / direct-attention / answer-position) plus WhatIfVis-style activation patching localized to object-position vs attribute-position encodings, on 3–5 small open VLMs; enemies: (i) encoding loss (patches at vision layers don't repair), (ii) attention confusion (direct-attention repairs), (iii) linguistic-prior dominance (SPC-prior protocol reproduces), (iv) nothing repairs (negative result).
3. **A-1 (reserve) — "Usage vs representation in CoT-level visual dropout."** Per-position measurement (attention mass to image tokens vs hidden-state shift upon image-token edit vs output-shift) across a reasoning trajectory on Qwen3-VL-2B/4B/8B; enemies: (i) attention explains (2608.01314-compatible), (ii) representation decay explains (2606.28864-compatible), (iii) neither — prefix-anchoring artifact. Flagged as range-touching existing work; chosen only if B-1/C-1 fail in practice.

Phase 1 closed. Awaiting Phase 2 instructions.