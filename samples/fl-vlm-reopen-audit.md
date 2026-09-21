# Adversarial Thesis Reopening Audit - Federated Learning + Medical Vision-Language Models (VLM variant)

Project: Thesis. Auditor role: skeptical senior reviewer.
Status of topic before this audit: FL is a PARKED direction; higher burden of proof required to reopen.
Audit date: 2026-09-20. Scope: federated PEFT of medical VLMs / multimodal foundation models, with a reliability/calibration lens.
Complements the non-VLM audit (`fl-reopen-audit.md` in this folder).

Load-bearing evidence carried in this audit: arXiv:2609.02101 (fully read), arXiv:2606.20115 (fully read), arXiv:2608.27004 / MVC-Bench (fully read), arXiv:2608.25251 (fully read), arXiv:2607.13386 / FM2 (read), arXiv:2608.02790 (read), arXiv:2605.08992 / FM Fairness Paradox (read). Model/dataset license facts verified by web search on 2026-09-20.

---

## 1. Executive Verdict

**KEEP PARKED (the VLM variant, as stated).** One reserve-grade question exists (RQ A below) but its premise is unproven; the reopening conditions are not met, and the nearest "gap" is already occupied along every single dimension by recent work. If - and only if - the 60-90 minute falsification task in section 21 flips positive should the direction upgrade to RESERVE (not REOPEN), limited to a single candidate (RQ A).

The reconstruction from the literature reads:

- Federated LoRA of a medical vision-language foundation model (BiomedCLIP) across FOUR real international chest-X-ray cohorts **already exists**: arXiv:2609.02101 (USA/Vietnam/Spain cohorts, ~0.687->0.802 shared-class AUC; product-space/FlexLoRA aggregation required over naive factor averaging; FedProx no better than FedAvg; centralized ref 0.812). Classification-only, single-seed, no reliability/calibration metrics.
- Real-site calibration failure under federation **already documented**, but in conformal segmentation, not VLMs: arXiv:2606.20115 (FeTS-2022, 1,251 subjects, 20 institutions; pooled CRC violates coverage at 40% of institutions; worst-site FNR budget exceeded by 7.8pp; site-conditional risk-curve shrinkage fixes it). Code public.
- Medical VLM calibration under distribution shift **already benchmarked centrally**: arXiv:2608.27004 MVC-Bench (1,638 controlled runs, 8 backbones, 3 modalities, ECE/MCE/ACE; MCM regularization best in-distribution; no significant positive calibration rank-transfer across shift).
- Federated foundation-model + PEFT **worst-client disparity already documented**: arXiv:2605.08992 FM Fairness Paradox (DistilBERT+LoRA worst-client gap 50.1% vs TextCNN 32.2% at alpha=0.1; inverse-weight aggregation does not fix it) - but text classification, simulated clients.

The only unoccupied cell is "federated LoRA of a GENERATIVE medical VLM + worst/unseen-client calibration". An empty cell is not a thesis; it is the absence of prior work. You would have to discover the phenomenon yourself, on simulated clients, after which several stronger groups' results would already bound the claim.

---

## 2. Kill Criteria (re-read filters applied this audit)

Any one of these implies KEEP PARKED:

1. Minimum meaningful experiment requires an A100 or multi-GPU training step. (Section 13: does NOT fire; Qwen2-VL-2B QLoRA fits a T4; BiomedCLIP ViT-B is tiny.)
2. Infrastructure effort dominates research effort. (Section 14: does NOT fire if single-machine Flower simulation is pinned to one version; same containment as the non-VLM audit.)
3. The question dies when you remove the word "federated". (Section 15: PARTIALLY FIRES. The calibration idea survives centrally as shift-aware calibration [MVC-Bench]; the FL framing is what is unclaimed, which is a red flag per section 4.)
4. No realistic public dataset with defensible client partitions exists. (Section 5: does NOT fire for the CXR-cohort stack [real cohorts, as in 2609.02101]; FIRES for medical-VQA specifically - no public multi-site VQA set.)
5. You cannot produce a falsifiable H0 that a 60-90 minute experiment could disprove. (Section 21: a task exists, but its most likely outcome is the null.)
6. (New) The closest-work catalogue already spans every sub-dimension (FL+AUC done; FL+calibration done; central VLM+calibration done; FL+VLM PEFT done), so no component is novel; only one unclaimed *intersection* remains. Intersection-novelty is weak novelty (section 4).

Three of six fire (3, 4-partial, 6) plus the premise is unproven -> KEEP PARKED.

---

## 3. Lab Inventory & Operating Constraints (re-confirmed)

- Compute: Ryzen 5 5600G, 8 GB RAM, no local CUDA; Colab Pro (T4/V100/A100 allocation varies per session).
- People: 3 undergraduate students, self-directed, thesis scope. Small commercial API budget.
- Stack allowed: PyTorch, PEFT/bitsandbytes, Flower, transformers, open_clip. No hospitals, no private data, no multi-node infrastructure.
- Data policy: public datasets only; clients must be real-cohort labels (FeTS, CXR cohorts) or documented simulations.

---

## 4. Novelty Test - is a "Federated Medical VLM reliability" thesis scientifically different from centralized PEFT?

The decisive question. "Train separately on several datasets, or federate, then evaluate calibration" is NOT a new idea. The difference that would justify a thesis must be: **aggregation itself creates a measurable, causal, reliability phenomenon that no centralized procedure (pooled PEFT, PEFT-per-site + average, PEFT-per-site + ensemble) reproduces at matched data volume.**

What the evidence actually supports:

- FM Fairness Paradox (2605.08992): under extreme label skew (alpha=0.1), pretrained FM + LoRA DID produce a worst-client gap (50.1pp) worse than a small task model (32.2pp), and inverse-size aggregation (FedAvgW) did NOT fix it. This is the closest thing to "aggregation creates a phenomenon" in the 2025-2026 literature - but it is text classification, simulated clients, no calibration metric, no medical data.
- Fed-CRC-Seg (2606.20115): pooled *conformal* calibration violates worst-site guarantees (40% of institutions), fixed by site-conditional risk curves. But this is a post-hoc calibration wrapper, not model aggregation; it shows the *evaluation* needs site-conditioning, not that federated training miscalibrates.
- MVC-Bench (2608.27004): centrally fine-tuned medical VLMs already show calibration rank-reversal across a domain-shift axis (0/30 significantly positive transfer) and MCM-reg regularization helps in-distribution. This is adverse pressure: calibration failure is already present in CENTRAL PEFT under shift, so a thesis claiming "federation causes miscalibration" must control for the central-shift artifact or it reads as "miscalibration under shift; federation irrelevant."

Interpretation (inference): the scientific-difference test today is UNMET for the VLM variant. You would be entering with "maybe aggregation does something on top", which is the wrong footing for a thesis that lives or dies on exactly that claim.

---

## 5. Public Dataset Candidates

| Dataset | Content / size | Client (site) labels? | License / access | Verdict for VLM variant |
|---|---|---|---|---|
| NIH ChestXray14 + CheXpert + PadChest + VinDr-CXR (4-cohort CXR stack) | Chest X-ray classification, 3 continents (USA, Vietnam, Spain) | REAL cohort labels (used as real clients by 2609.02101) | NIH public; CheXpert research-use; PadChest CC BY-NC-SA; VinDr CC BY-NC | Best real-client spine, but classification-biased; no free text -> weak VQA fit |
| VQA-RAD | 2,244 QA pairs / 314 images (MedPix); clinician-generated | No (single collection) | CC0 1.0 (OSF; verified) | Cheap generative-VLM QA spine; tiny; single site |
| SLAKE | 642 images / 14,028 QA pairs, bilingual, knowledge-base | No | CC BY-NC-SA 4.0 (arXiv HTML) | Cheap QA spine; single site; NC-only |
| PathVQA | ~149K pathology images / ~33K QA | No | Public | QA scale, but single source, no site split |
| Medical-CXR-VQA / MIMIC-CXR | 143K images / 377K QA, CXR | Could map hospital IDs | PhysioNet credentialed + DUA -> NOT usable by an uncredentialed team | Excluded on access |
| FeTS-2022 | 3D brain mpMRI, ~1,251 train subjects, 4 modalities, glioma | YES - real per-institution partition (23-33 institutions; ~61% with <15 samples) | Non-commercial; Synapse syn28546456; manual approval | The only real-site-labeled set, but MRI segmentation without text -> poor VLM fit; reuse pretrained weights for calibration-only checks |
| HAM10000 / ISIC | 2D dermoscopy, 10,015 images, 7 classes | Two real sites (Vienna, Queensland) in the paper, no per-image site column | CC BY-NC 4.0 | 2D fallback; must simulate + patient-stratify (leakage warning 2401.14497) |
| MedMNIST v2 / DermaMNIST | 708,069 standardized 2D + 3D slices | None | CC BY 4.0 (DermaMNIST CC BY-NC) | Debug/infra only - too clean to carry a claim |

Verdict (inference): there is NO public dataset with real multi-site labels AND image+text VQA content. The CXR stack gives real clients but no text; medical-VQA sets give text but one site. Any "federated medical VLM" study therefore either (a) bolts synthetic QA text onto the CXR stack (real clients, synthetic text; API budget) or (b) simulates client skew on VQA-RAD/SLAKE (synthetic clients). Both are honest only if framed as simulated federation (section 10), and 2609.02101 already took path (a) without reliability metrics.

---

## 6. Model Candidates (verified facts)

| Model | Size / arch | License (verified) | T4 (16 GB) feasibility | PEFT |
|---|---|---|---|---|
| Qwen2-VL-2B / 7B-Instruct | 2.21B / 7.6B multimodal | Apache 2.0 for 2B & 7B (72B is Qwen license) | 2B: YES (4-bit NF4 QLoRA ~2-6 GB; T4 fine-tuning precedent ~850 steps for 1.5B class); 7B tight (BF16 ~16 GB; use 4-bit + grad checkpointing) | LoRA/QLoRA native via transformers |
| LLaVA-1.5-7B | 7B, Vicuna/Llama-2 base | Weights commonly research-use; base Llama-2 license applies - NOT publication-safe for a thesis | LoRA fine-tune fits with 4-bit; slow on T4 | LoRA (scripts exist) |
| LLaVA-Med (13B) | Vicuna-13B | LLaMA research license - NOT publication-safe | No (13B LoRA marginal on T4) | LoRA possible |
| BiomedCLIP (ViT-B/16 + PubMedBERT-256) | ~450M total (86M ViT-B) | MIT per community HF mirror (official microsoft card lists no license tag - flag in thesis); arXiv 2303.00915 | Trivial; whole model tens of MB-GB | LoRA / linear-probe; this is what 2609.02101 federated |
| LLaVA-OneVision-1.5 (4-8B) | Apache-2.0 open multimodal framework | Apache 2.0 | 4B: yes with QLoRA | full/LoRA |

Recommendation: if this direction ever reopens, use Qwen2-VL-2B (Apache-2.0) for generative-VLM claims and BiomedCLIP for the cheap classification spine. Avoid Vicuna/LLaMA-derived weights in a thesis artifact on licensing grounds.

---

## 7. Best Federated Framework

Same verdict as the non-VLM audit, re-applied to PEFT:

| Framework | Verdict |
|---|---|
| Flower | Keep. Mature single-machine Simulation Runtime (Ray-based virtual clients). High API churn -> pin one version, follow only the "simulations" docs |
| FedML | Weakened - org pivoted to a commercial platform; use only old recipes |
| PySyft | No - rewritten into private-compute-over-file-transports; classic FL orchestrator deprecated |
| Manual loop / microsoft fl-simulation | Strongest for a thesis: full transparency of the aggregation math, especially LoRA product-space merging |

For LoRA aggregation the framework matters less than the aggregation operator: 2609.02101 shows naive factor averaging costs ~0.097 AUC vs product-space (FlexLoRA) aggregation. Flower alone does not implement product-space LoRA merging - you must implement it and unit-test it (section 8).

---

## 8. Artifact Evaluation

A defensible artifact bundle for any candidate that passes the gates:

1. Reproducible harness: pinned versions, >=3 fixed seeds, per-run metric JSONs (mean + worst-client), Colab notebook + CLI backend with identical codepath.
2. Aggregation operator module: FedAvg / product-space LoRA (FlexLoRA-style SVD merge), with a unit test proving merge-then-eval ~= separate-then-eval for a linear factor case.
3. Calibration toolkit: ECE/MCE/ACE with pooled-vs-site-conditional variants (conformal wrapper replicating Fed-CRC-Seg's risk-curve shrinkage), risk-coverage curves, abstention evaluation.
4. Honesty sheet: real (FeTS/CXR cohort) vs simulated client split; what was pre-registered; which baselines matched federated compute; how leakage was handled.
5. Negative-result section: the null ("central-pooled reproduces everything") must be a first-class citizen.

---

## 9. Can the Manual Phase Stand Alone?

Yes - and that is a trap. The manual single-machine simulation phase (2D + small QA, manual FL loop) can absolutely stand alone and produce numbers in a week. The problem: that phase proves nothing about the real claim; it is exactly the phase where an aggregation-caused miscalibration phenomenon is least likely to appear (small data, homogeneous errors, few rounds), so a null there is uninterpretable ("phenomenon absent" vs "simulation too weak"). Manual phase = iteration loop and falsification tool (section 21), never the thesis claim itself.

---

## 10. Scientific Validity of Simulated Federation (minimum conditions to claim anything)

If clients are simulated (there are no public real-site VLM-with-text datasets), the thesis must state and satisfy, in writing:

1. Client construction is acquisition-grounded, not random: graded appearance shift (brightness/contrast, resolution, blur, noise) and/or label/population skew (patient subgroup mixes), ranked per a data-centric non-IID taxonomy. Bare Dirichlet label skew is an artifact, usable only as a control condition.
2. Matched-data control is mandatory: federated (K clients, R rounds) vs central-pooled on the SAME union with MATCHED optimizer step/epoch budget; otherwise every gap is a data-scarcity artifact.
3. Worst-client/unseen-client are the primary axes; means are never reported alone. Report per-client ECE/ACC distributions, coverage of a 90% risk budget per site, risk-coverage curves.
4. Patient-level stratification (leakage warning for HAM10000/DermaMNIST-derived splits).
5. Compute honesty: report total GPU-minutes federated vs central; if federated costs >3x central for equal quality, that is itself the headline.
6. No "real federation" claims - simulated federation is a stressor battery, cited as such.

---

## 11. What Would Make This a Reasonable DL Thesis

A reasonable (not glamorous) thesis here is a controlled characterization with a pre-registered null, answering ONE of: (i) does federated PEFT of a medical VLM miscalibrate minority/unseen clients beyond central PEFT of the same data (aggregation-specificity); (ii) at what heterogeneity level does the FM/LoRA worst-client disparity (FM Fairness Paradox) appear in a medical VLM; (iii) does cross-cohort calibration repair transfer at all (MVC-Bench rank-reversal extension with a fix). Reasonableness comes from a crisp IV/DV matrix, matched-compute controls, worst-client-first reporting, and honesty to publish a null.

---

## 12. Ideal Controlled Experiment (the 8-run matrix for the reserve candidate)

For RQ A specifically, the minimal controlled design is an 8-run matrix (2^3, one run per seed is a minimum of 2-3 seeds each = 16-24 runs):

- Factor 1 - model: (a) BiomedCLIP LoRA classification vs (b) Qwen2-VL-2B QLoRA generative VQA.
- Factor 2 - training regime: (i) federated (FedAvg + product-space LoRA merge) vs (ii) central-pooled at matched epochs/steps.
- Factor 3 - client regime: (I) real-cohort CXR stack (4 clients, real site labels) vs (II) simulated skew on VQA-RAD/SLAKE (label + appearance shift).

Primary DVs: worst-client ECE and unseen-client accuracy/risk-coverage. Secondary: mean ECE, per-client gap, calibration rank-transfer. Baselines added as rows: per-site isolated PEFT (no aggregation), FedProx, and a shift-aware centralized calibrator (MVC-Bench-style). Pre-register effect sizes and the matched-compute budget.

This is exactly the kind of matrix that would settle H0-vs-H1, which is one more reason the falsification task (section 21) should run FIRST: you should not build the full matrix until the cheap test says the phenomenon can appear at all.

---

## 13. Compute Feasibility / Kill Gate

Gate: minimum meaningful experiment must fit within ~120 minutes of collective training time on a single commercial GPU (T4-class), else the direction is infra-limited for an undergraduate team; sustained workloads must fit the semester inside the 8 GB/Colab envelope.

| Workload | T4 (16 GB) feasibility |
|---|---|
| BiomedCLIP LoRA classification (ViT-B) | YES, minutes per run |
| Qwen2-VL-2B QLoRA generative VQA on VQA-RAD/SLAKE (2-16k QA) | YES, ~10-25 min per run (10-25 steps of gradient accumulation, short context) |
| Qwen2-VL-7B / LLaVA-1.5-7B QLoRA | Marginal-slow (BF16 ~16 GB); slow on T4 |
| Full 8-run matrix x 3 seeds (section 12) | ~1-2 hours of training total -> borderline acceptable but eats the kill gate; the falsification task must run first |
| FeTS-2022 calibration-only with pretrained weights | YES (inference + risk-curve computation only) |

Verdict: the minimum falsification experiment fits inside the gate; the full thesis matrix roughly meets it only if executers stay disciplined with VQA-RAD-class sizes. 7B generative models must NOT be the default - they blow the gate and add nothing the 2B arm cannot test.

---

## 14. Infrastructure Risk

Containment identical to the non-VLM audit (single-machine simulation, Flower pinned, manual loop primary) plus three VLM-specific risks:

1. LoRA aggregation correctness (product-space vs factor-wise) - subtle math, easy silent bug that flips conclusions; unit-test the merge before trusting any result.
2. Token/VRAM headroom - generative pipelines are memory-fragile on T4; 4-bit + gradient checkpointing + short context mandatory.
3. Flower version churn - pin one version; simulate with the manual loop and treat Flower as an optional compatibility layer.
4. PhysioNet credentialed datasets (Medical-CXR-VQA/MIMIC) are off-limits for an uncredentialed team - do not design around them.

Verdict: infrastructure is containable but not free; cap at about 1-2 weeks of the pilot, matching the non-VLM audit.

---

## 15. Why Federation? - Test 1 (three questions)

Q1. What is lost if a central server can accurately reconstruct the exact federation from the same public data? Answer: nothing - all four CXR cohorts and VQA sets are public. The rationale collapses unless the thesis demonstrates that the fed-trained model retains some property a central-pooled model on the same data lacks (this is precisely the unproven premise of section 4).

Q2. What evidence exists of a real-world problem that specifically requires real data partitioning - not just data scarcity or limited compute? Evidence exists in the clinical conformal literature (Fed-CRC-Seg shows pooled coverage fails worst hospitals on real FeTS-2022) but NOT yet for VLMs. Transferring that pattern to VLM behavior is an inference, not a demonstration.

Q3. What unique phenomenon does aggregation (not merely multi-site data) produce? Candidate: worst-client miscalibration on unseen clients (FM Fairness Paradox precedent, NLP-only). Not demonstrated for medical VLMs anywhere. Until the falsification task shows it, Q3 has no affirmative answer in this domain.

Verdict: "Why federation?" fails all three in the affirmative for the VLM variant today; only Q2 has a nearby precedent that is non-VLM. The parking rule was designed for exactly this: federation must buy more than realism.

---

## 16. Why VLM? - Test 2 (three questions)

Q1. What does a multimodal VLM give over a unimodal baseline (e.g., plain ViT + linear head, or appending a small LLM head)? For QA tasks, a generative VLM is required to emit free-form answers, so the VLM is not decorative there - but then the interesting reliability object is open-ended generation, whose shared-class-AUC chest-X-ray analog (2609.02101) already exists for BiomedCLIP.

Q2. Is the VLM the object of the reliability claim (a claim ABOUT the VLM), or is it a tool? For a real thesis the VLM should be the object: e.g., "medical VLMs reproduce the FM-Fairness-Paradox worst-client behavior under federation." That framing is testable; "use a VLM as a tool to do FL" is only tool-chain novelty.

Q3. Does the reliability failure persist without the language component (i.e., is it really a multimodal phenomenon)? If a ViT-only federated LoRA already shows the worst-client ECE gap, the VLM adds nothing and the thesis collapses to non-VLM FL (the sibling audit's territory, where Fed-CRC-Seg already sits).

Verdict: the VLM is only defensible in the QA/QA-generation formulation AND only if the failure the thesis measures cannot be reproduced with a unimodal encoder under the same federation.

---

## 17. Closest-Work Autopsy (45-minute verification)

Closest work: arXiv:2609.02101 "Federated LoRA Adaptation of BiomedCLIP Across Four International Chest X-Ray Cohorts."

What I verified from the paper text: 4 public CXR cohorts across 3 continents; shared-class AUC improves ~0.687 -> 0.802 with federated LoRA vs 0.812 centralized-pooled reference; naive factor averaging drops mean AUC by ~0.097 relative to product-space (FlexLoRA) aggregation - a large, direct empirical argument that THE AGGREGATION OPERATOR ITSELF MATTERS; FedProx gives no advantage over FedAvg; single-seed experiments.

What it does NOT do (the residual space): generative medical VLM (it is contrastive CLIP-style, classification head); reliability/calibration metrics (accuracy only); multi-seed / uncertainty quantification; unseen-client generalization evaluation (evaluates the same cohorts it was fine-tuned on, in-shift plus a held-out share); no worst-client framing.

Why it is not your thesis: it already demonstrates aggregation specificity on the exact spine you'd use (CXR cohort LoRA). Your added axes (generative VLM, calibration) are open, but the burden is on you to show (section 4, 13, 21) that those axes interact with federation to produce a NEW phenomenon rather than replicate MVC-Bench's central-shift miscalibration.

Second-closest (calibration axis): arXiv:2606.20115 on FeTS-2022 real institutions - the reliability framing you'd want is occupied in segmentation form, and its site-conditional calibration recipe is directly importable to a VLM that also emits confidence. Combined with the segmentation result: fed-VLM calibration is only defensible if the conformal result does not already answer the question (it answers part of it - site-specific calibration failure is real).

Follow-up risk since reading: both papers are 2026, no published follow-ups found in the discovery run; treat them as a moving target and re-run the audit before submission.

---

## 18. What a "Principle" Looks Like

A defensible thesis product here is a principle of the form: "Under [heterogeneity model], federated PEFT of a [model family] produces [reliability outcome] on minority clients; the outcome is [aggregation-intrinsic / remedied-by] [mechanism] - and this reproduces across [>=2 tasks/datasets]." Examples of principle-grade claims (testable): (i) FM-Fairness-Paradox-like worst-client calibration gap reproduces under federated LoRA of medical VLMs; (ii) calibration rank-transfer across cohorts is negative for generative medical VLMs, and MCM-style training flips it partially; (iii) product-space LoRA aggregation is necessary for calibration parity, not only AUC. A benchmark-with-a-delta is not a principle.

---

## 19. When a Naive Combination Is a Red Flag

Red-flag patterns (each present in at least one candidate configuration of this topic):

1. Every component is an independently hot topic and no component claim is new: FED (hot) + VLM (hot) + MEDICAL (hot) + CALIBRATION (hot). Stacking four mature problems does not create novelty; the intersection must be a working, falsifiable phenomenon.
2. The aggregation operator is treated as commodity (plain FedAvg over LoRA factors) when the closest work proves the operator choice alone swings mean AUC by ~0.1 (2609.02101).
3. Client construction is a bare Dirichlet split with no acquisition-grounding, and the thesis still uses the word "hospital".
4. The calibration story is evaluated only on in-distribution held-out data, never on an unseen cohort - which is where MVC-Bench says the calibration breaks.
5. "Simulated federation" is presented as a strength instead of a limitation to be bounded by matched-compute controls.

---

## 20. Reopening Conditions (all must hold to reopen)

Audit rule: FL reopens only if every condition below holds; count the failures honestly:

1. A persuasive answer to section 4 (aggregation creates a measurable phenomenon on the same public data) - UNPROVEN.
2. A persuasive answer to Why-Federation Q2 (real problem needing partitioning, with prior evidence in-domain) - PARTIAL (Fed-CRC-Seg is segmentation; no VLM).
3. A persuasive answer to Why-VLM Q3 (phenomenon persists only because of the language component, not a ViT-only encoder) - UNTESTED.
4. A public dataset with real client partitions AND image+text QA content - ABSENT (real clients xor real text today).
5. Compute evidence that the full evidence matrix fits the 120-minute gate - UNMAPPED beyond the minimum falsification; the 8-run matrix is borderline.
6. Pre-registered, matched-compute controls including per-site-isolated and central-pooled baselines - TRIVIALLY ACHIEVABLE but unplanned so far.
7. Worst-client/unseen-client primary metrics and honest negative-result reporting - ACHIEVABLE (reporting discipline).
8. Re-run audit confirms the closest work has not migrated onto the residual cell since 2026-09-20 - UNKNOWN; must be re-verified at submission.
9. Licensing is publication-safe (Apache-2.0 Qwen2-VL + certified-license data) - ACHIEVABLE (if MIMIC/LLaVA-derived are excluded).
10. The team can state exactly which 1-2 contributions are theirs vs the 5 anchor papers - NOT YET WRITTEN; currently it would read "generative-VLM + calibration," which is thin.

Count today: 3 clearly unmet (1, 4, 5), 3 only partial (2, 3, 8), 4 achievable (6, 7, 9, 10). Because 1 (the load-bearing premise) and 4 are unmet, the direction stays PARKED regardless of the achievable ones.

---

## 21. Final Verdict + The 60-90 Minute Falsification Task

**Verdict: KEEP PARKED (VLM variant).** Reserve status possible ONLY if the falsification task below flips positive. One reserve-grade candidate survives the audits:

**RQ A (reserve):** Under simulated-but-acquisition-grounded client heterogeneity, does federated LoRA adaptation of a generative medical VLM (Qwen2-VL-2B) on medical QA produce worst-client calibration failure that central-pooled PEFT on the same data does not reproduce?
- H0: worst-client ECE(gap) under federation equals worst-client ECE of central-pooled PEFT at matched steps (aggregation adds nothing).
- H1: federation produces a reliably larger worst-client/unseen-client ECE gap at matched compute.
- Variables: IV = training regime (federated vs central-pooled) x heterogeneity level; DV = worst-client ECE, per-client ECE spread, unseen-client accuracy, risk-coverage.
- Intervention: FedAvg + product-space LoRA merge; control: central-pooled with identical data and matched optimizer budget; per-site isolated; FedProx; MVC-Bench-style shift-aware calibrator as upper/lower reference.
- Metrics: ECE/MCE, per-client spread, coverage at 90% risk budget, rank-transfer.
- Expected under H1: worst-client gap grows monotonically with heterogeneity while mean stays flat (FM-Paradox shape).
- Negative result that kills it: mean-and-worst both move together with data volume, and central-pooled reproduces the gap - then federation is a red herring and the direction is refuted.

**The 60-90 minute falsification task (run FIRST, alone, before anything else):**
- Colab T4; Qwen2-VL-2B QLoRA (4-bit, rank 16-32) or even a linear-probe classifier on BiomedCLIP features as the zero-cost arm.
- Take VQA-RAD + SLAKE QA pairs (or CXR cohort labels for the classifier arm). Create 4 simulated clients with documented label skew + appearance shift (contrast/brightness/blur), patient-stratified (<=150 training samples per client).
- Train: (1) federated FL over 4 clients, 5-10 rounds, product-space LoRA merge; (2) central-pooled on the union with equal total optimizer steps; (3) matched-calorie per-site solo reweighted average.
- Measure worst-client ECE, unseen-client (held-out 5th site) accuracy and risk-coverage on all three.
- Thrive rule: fed worst-client ECE exceeds central by >2pp (multiple seeds) AND the per-site solo baseline does not explain it -> upgrade direction to RESERVE, run the section-12 8-run matrix, qualify Q2/Q3.
- Kill rule: central-pooled matches or beats fed on worst-client ECE within noise, or the gap vanishes at matched compute -> the imposed premise is refuted, and the direction stays PARKED. Expected most-likely outcome per section 4: the kill rule fires.

Any higher-level decision (including reopening) stays gated on the 10 conditions in section 20, which are not met today.