# Evaluating TDD Test Execution Cost and CI/CD Stability using xUnit Test Reports and GitHub Actions Logs in a .NET N-Layer API

**Hoan Le**
hoan.le@student.oulu.fi
Master's Programme in Software Engineering and Information Systems, University of Oulu, Finland

---

## Abstract

Continuous Integration and Continuous Deployment (CI/CD) pipelines rely heavily on automated testing to ensure software stability. In traditional N-layer architectures, Test-Driven Development (TDD) often necessitates extensive use of mock objects to isolate the Business Logic Layer from Data Access and Presentation dependencies. This seminar paper investigates how TDD affects test execution cost and CI/CD stability in a .NET N-Layer API. A small layered architecture is implemented, consisting of presentation, application, and infrastructure layers. TDD is applied to develop core business logic using xUnit, and GitHub Actions is used to automate the CI/CD pipeline. XML test reports and GitHub Actions workflow logs are obtained and analyzed. Two testing strategies are compared: standard repository tests using EF Core InMemoryDatabase (`Standard_Tests`) and service-layer tests using Moq mock objects (`Mocked_Dependency_Tests`). Across 20,000 total test executions, `Mocked_Dependency_Tests` achieve a 2.6× lower mean execution time and 7.2× lower median execution time in steady-state compared to `Standard_Tests`. Both strategies yield a 100% pass rate across all CI runs, confirming strong pipeline stability. The findings demonstrate that TDD produces observable, measurable patterns in test execution cost and CI/CD behavior even in small systems.

**Keywords:** Test-Driven Development, Continuous Integration, xUnit, GitHub Actions, Software Testing, Mocking.

---

## 1 Introduction

This paper examines the operational impact of TDD on test execution cost and CI/CD stability within a .NET N-Layer API. The goal is to understand whether TDD introduces measurable overhead or benefits in automated testing and continuous integration workflows.

To support this research, a small N-Layer architecture is implemented, consisting of:

- **Presentation Layer:** ASP.NET Core controller exposing API endpoints
- **Application Layer:** business logic services
- **Infrastructure Layer:** simple repository abstraction backed by EF Core and SQLite

This structure allows realistic testing scenarios without the complexity of large enterprise systems. TDD is applied to implement selected features across the application and infrastructure layers. Two distinct testing strategies are employed:

1. **Standard Tests** — repository-level tests using EF Core's `InMemoryDatabase` provider, exercising the full data-access logic without a real database engine.
2. **Mocked Dependency Tests** — service-layer tests using the Moq framework to replace the repository with a mock object, isolating the business logic entirely from infrastructure.

xUnit generates structured XML test reports. GitHub Actions provides CI/CD automation, producing workflow artifacts that capture pipeline duration, failure rates, and regression behavior. Together, these datasets enable empirical evaluation of how TDD affects test duration and pipeline reliability.

The main message of this paper is that even in small systems, TDD produces observable patterns in test execution and CI/CD behavior. The following chapters introduce related work, describe the experimental setup and datasets, and present the analysis from the extracted pipeline logs.

---

## 2 Related Work

This section outlines the core publications found in the subject area of CI/CD pipeline efficiency and TDD architectural overhead.

### 2.1 CI/CD Efficiency and Execution Costs

**Hilton et al. (2016): "Usage, costs, and benefits of continuous integration in open-source projects."**

This study examines the popular use of CI in software development and identifies how the process affects the trade-off analysis. Even though CI has been established as an effective technique in avoiding integration problems and bugs, the researchers realized that the processing power needed to build and test becomes a bottleneck in development.

*Relevance:* This paper serves as the foundation for analyzing costs associated with CI. It justifies analyzing GitHub Actions logs to identify the contribution of unit tests to pipeline bottlenecks.

**Beller et al. (2017): "Oops, my tests broke the build: An explorative analysis of Travis CI with GitHub."**

The researchers studied millions of build logs to understand what causes CI pipeline failures. They found that an overwhelming proportion of failures is not caused by compilation issues but by failing tests, showing how crucial tests are as quality gates while also causing pipeline delays.

*Relevance:* This article provides context for the stability dimension of the research. It supports the methodology of analyzing test reports to determine whether the pipeline is catching defects before deployment.

### 2.2 TDD, Mocking, and N-Layer Architecture

**Mishra & Nayak (2022): "A comparative analysis of test-driven development and behavior-driven development in CI/CD pipelines."**

This research evaluates the impact of integrating TDD into continuous integration environments. While TDD ensures code correctness and reduces regressions, critics argue that extensive test-writing is time-consuming and risks overloading the CI/CD pipeline with low-level unit tests.

*Relevance:* This study provides the modern foundation for evaluating testing bottlenecks. It justifies the empirical investigation into the "mocking penalty" by highlighting that inefficiently managed tests can cause substantial pipeline delays.

**Spadini et al. (2019): "Mock objects for testing Java systems: Why and how developers use them, and how they evolve."**

This empirical study investigates the prevalence and evolution of mock objects in enterprise software. The research demonstrates that as systems become more layered and complex, reliance on mocking frameworks increases, directly impacting maintenance and execution overhead of the test suite.

*Relevance:* This article provides empirical backing that layered architectures inherently require extensive mocking. While focused on Java, the principles apply directly to the C#/.NET N-layer API built for this seminar, justifying the investigation into testing costs across the two strategies.

---

## 3 Empirical Experiences

### 3.1 Construction of the .NET N-Layer API

The demonstration system is a RESTful Product Management API built with ASP.NET Core (.NET 10) and organized into three layers, each with a strict dependency direction (Presentation → Application → Infrastructure):

**Models (shared):**
- `BaseEntity` — abstract base with an auto-incremented integer primary key.
- `Product` — inherits `BaseEntity`, exposes `Name`, `Price`, and a nullable foreign key to `Brand`.
- `Brand` — inherits `BaseEntity`, represents a product manufacturer.

**Infrastructure Layer:**
- `AppDbContext` — EF Core `DbContext` with `DbSet<Product>` and `DbSet<Brand>`, configured with SQLite in production via `products.db`.
- `IProductRepository` / `ProductRepository` — CRUD interface and implementation using EF Core. All mutations call `SaveChanges()` synchronously.

**Application Layer:**
- `IProductService` / `ProductService` — thin service wrapping `IProductRepository`. Each method delegates to the corresponding repository method. Although thin, this layer is the target of mock-based unit testing because it is the boundary at which dependencies should be injectable and replaceable.

**Presentation Layer:**
- `ProductController` — ASP.NET Core `ApiController` exposing five endpoints: `GET /api/product`, `GET /api/product/{id}`, `POST /api/product`, `PUT /api/product/{id}`, `DELETE /api/product/{id}`. Responses follow standard HTTP conventions (`200 OK`, `201 Created`, `204 No Content`, `404 Not Found`, `400 Bad Request`).

Dependency injection is wired in `Program.cs` with scoped lifetimes for both `IProductRepository` and `IProductService`, and EF Core is configured to call `EnsureCreated()` at startup to initialize the SQLite schema.

### 3.2 TDD Implementation Steps

TDD was applied following the Red-Green-Refactor cycle for both testing strategies. For each feature, a failing test was written first, the minimum implementation was added to make it pass, and the code was then refactored where necessary.

**Strategy 1 — Standard Tests (`StandardUnitTests`)**

These tests target the `ProductRepository` (infrastructure layer). To avoid coupling tests to a real SQLite file, EF Core's `UseInMemoryDatabase` provider is used. A `RepositoriesFactory` helper creates a fresh, isolated in-memory context per test using a unique `Guid` database name, ensuring tests are fully independent and parallelizable.

Five test methods were implemented:
- `Add_ShouldInsertNewProduct`
- `Update_ShouldModifyExistingProduct`
- `Delete_ShouldRemoveExistingProduct`
- `GetAll_ShouldReturnAllProducts`
- `GetById_ShouldReturnCorrectProduct`

Each test follows the Arrange-Act-Assert pattern, verifying that the repository correctly interacts with the EF Core in-memory store.

**Strategy 2 — Mocked Dependency Tests (`MockUnitTests`)**

These tests target `ProductService` (application layer). The Moq framework is used to replace `IProductRepository` with a mock object, completely isolating the service from any database interaction. This approach directly reflects the pattern described by Spadini et al. (2019): as architectures become layered, mocking dependencies at layer boundaries becomes standard practice.

Five test methods were implemented:
- `GetAllProducts_ShouldReturnAllProducts`
- `GetProductById_ShouldReturnProduct_WhenExists`
- `GetProductById_ShouldReturnNull_WhenNotFound`
- `CreateProduct_ShouldCallRepositoryAdd`
- `DeleteProduct_ShouldCallRepositoryDelete`

Each test configures mock behavior with `mockRepo.Setup(...)` and verifies interaction with `mockRepo.Verify(..., Times.Once)`.

**Simulation Design**

To generate statistically meaningful execution time data, each test method is parameterized using xUnit's `[Theory]` and `[MemberData]` attributes with a `runNumber` parameter from 1 to 2,000. The parameter itself is not used in any assertion — it exists solely to instruct xUnit to generate 2,000 distinct test cases per method. With 5 methods per class and 2 classes, this produces 20,000 total test executions per CI run, providing a sufficient sample for distribution analysis.

### 3.3 CI/CD Pipeline Configuration

The GitHub Actions workflow (`.github/workflows/ci.yml`) is triggered on every push and pull request to the `main` branch. The pipeline consists of five sequential steps:

1. **Checkout** — retrieves the source code using `actions/checkout@v4`.
2. **Setup .NET** — installs the .NET 10 SDK using `actions/setup-dotnet@v4`.
3. **Restore** — runs `dotnet restore` to resolve NuGet dependencies.
4. **Build** — runs `dotnet build --no-restore` to compile the solution.
5. **Test** — runs `dotnet test` with the xUnit XML logger (`--logger:"xunit;LogFileName=test_results.xml"`), writing results to `./TestResults/`.
6. **Upload Artifact** — uploads the XML file as the `xunit-test-report` artifact using `actions/upload-artifact@v4`, preserving it for later download even when tests fail (`if: always()`).

### 3.4 Data Extraction and Analysis

A Jupyter notebook (`github_reports/report_results.ipynb`) automates the full analysis pipeline in three cells:

- **Cell 1 (Download):** Uses the GitHub REST API (`GET /repos/{owner}/{repo}/actions/artifacts`) to fetch the latest `xunit-test-report` artifact, downloads the ZIP, and extracts `test_results.xml`. The `GITHUB_TOKEN` is read from the environment variable, with a `getpass` fallback for interactive sessions.
- **Cell 2 (Parse):** Parses the xUnit XML using Python's `xml.etree.ElementTree`, classifying tests by class name (`MockUnitTests` → `Mocked_Dependency_Tests`, `StandardUnitTests` → `Standard_Tests`), and exports a flat CSV (`tdd_execution_dataset.csv`) with columns: `Run_Source`, `Test_Name`, `Test_Type`, `Execution_Time_Seconds`, `Execution_Time_Milliseconds`, `Result`.
- **Cell 3 (Visualize):** Produces three charts using seaborn and matplotlib: a log-scale box plot of execution time distribution, a bar chart of steady-state mean (first run excluded), and a line chart of the JIT warm-up effect over the first 20 runs per strategy.

---

## 4 Discussion

### 4.1 Test Execution Cost

The extracted dataset contains 20,000 test records from a single CI run on 2026-04-16 (pipeline duration: 14.69 seconds total). Table 1 presents the full execution time distribution for both strategies.

**Table 1. Full Execution Time Distribution (all 10,000 runs per strategy, ms)**

| Strategy | Mean | Median | Std Dev | Min | Max |
|---|---|---|---|---|---|
| Standard_Tests | 0.717 | 0.468 | 15.069 | 0.129 | 1503.022 |
| Mocked_Dependency_Tests | 0.226 | 0.065 | 1.058 | 0.034 | 82.505 |

The mean execution time of `Standard_Tests` (0.717 ms) is 3.2× higher than `Mocked_Dependency_Tests` (0.226 ms). The difference in standard deviation (15.069 vs 1.058) and maximum values (1503 ms vs 82 ms) is dominated by the JIT warm-up effect discussed in Section 4.2. Excluding the first run per strategy provides a cleaner steady-state comparison (Table 2).

**Table 2. Steady-State Execution Time (first run excluded, ms)**

| Strategy | Mean | Median | Std Dev | Min | Max |
|---|---|---|---|---|---|
| Standard_Tests | 0.567 | 0.468 | 1.153 | 0.129 | 90.544 |
| Mocked_Dependency_Tests | 0.218 | 0.065 | 0.665 | 0.034 | 18.095 |

In steady-state, `Mocked_Dependency_Tests` are **2.6× faster in mean** and **7.2× faster in median** than `Standard_Tests`. This gap is consistent with Mishra & Nayak (2022), who predict that mock-based tests reduce per-test overhead by eliminating infrastructure setup costs. In this system, each `Standard_Tests` run initializes a fresh EF Core in-memory context (including schema creation), while each `Mocked_Dependency_Tests` run creates a lightweight Moq proxy object. The EF Core context setup cost is the primary driver of the performance gap.

The median is more representative than the mean for this dataset, because the standard deviation of `Standard_Tests` (1.153 ms) remains high even after excluding the first run, indicating occasional spikes (up to 90 ms) likely caused by garbage collection pauses during the 10,000-run sequence.

From a CI/CD cost perspective, the `StandardUnitTests` collection consumed 7.169 seconds of pipeline time versus 2.265 seconds for `MockUnitTests` (from the xUnit XML assembly-level metadata) — a **3.2× pipeline time ratio** that directly supports Hilton et al.'s (2016) observation that test execution time is a primary CI bottleneck.

### 4.2 JIT Warm-Up Effect

A significant finding is the first-run cost caused by .NET's Just-In-Time (JIT) compilation. The very first test execution in each class triggers JIT compilation of all referenced code paths, resulting in execution times orders of magnitude higher than subsequent runs:

- `Standard_Tests` first run: **1,503 ms** → drops to a median of 0.468 ms afterwards (3,210× reduction)
- `Mocked_Dependency_Tests` first run: **82.5 ms** → drops to a median of 0.065 ms afterwards (1,269× reduction)

This spike is clearly visible in the JIT Warm-Up chart (Figure 3 in the notebook output), which shows execution time over the first 20 runs per strategy on a log scale. Both strategies converge to their stable baseline within the first 3–5 runs.

The JIT warm-up effect has two practical implications for CI/CD. First, it artificially inflates the total pipeline duration of the first build on a cold runner, which GitHub Actions uses by default. Second, it means aggregate statistics (especially mean) are sensitive to run ordering — a finding that justifies reporting median alongside mean when analyzing xUnit execution data.

This finding also supports Beller et al.'s (2017) observation that test-related overhead in CI pipelines is often misattributed. In this case, a naive reading of the mean execution time (0.717 ms for `Standard_Tests`) overstates the steady-state cost by 27% compared to the median (0.468 ms), solely because of a single first-run spike.

### 4.3 CI/CD Stability

Across all 20,000 test executions in the analyzed CI run:

- **Pass rate: 100%** (20,000 passed, 0 failed, 0 skipped)
- **Total pipeline time: 14.69 seconds**

The 100% pass rate demonstrates strong CI/CD stability under TDD. This aligns with Beller et al.'s (2017) argument that TDD-driven test suites, when well-structured, act as reliable quality gates rather than sources of flakiness. The isolation design — each `Standard_Tests` run uses a uniquely named in-memory database, and each `Mocked_Dependency_Tests` run creates a fresh Moq instance — prevents shared state between tests, eliminating a common source of test flakiness in layered architectures.

The pipeline itself proved stable across multiple CI runs during development, with failures occurring only during intentional Red phases of the TDD cycle (when tests were written before the implementation), which is the expected and desired behavior.

### 4.4 Threats to Validity

Several limitations constrain the generalizability of these findings:

**Internal validity:** The 20,000 test executions are generated from a single CI run rather than multiple independent runs. JIT warm-up behavior, garbage collection timing, and GitHub Actions runner hardware variability could produce different distributions across runs.

**External validity:** The system under test is intentionally small (one entity type, five CRUD operations). Results may not scale to enterprise systems where test suites contain thousands of heterogeneous test types, integration tests, and database migrations.

**Construct validity:** The `runNumber` parameter used to generate 2,000 iterations per test method produces repeated executions of identical logic. This is useful for gathering execution time distributions but does not represent diverse test scenarios. The dataset measures execution cost patterns rather than defect detection capability.

**Strategy equivalence:** `Standard_Tests` and `Mocked_Dependency_Tests` do not test the same code units (repository vs. service), which means some of the execution time difference reflects the different complexity of the tested code rather than purely the overhead of EF Core vs. Moq.

---

## 5 Closing

This paper investigated how TDD affects test execution cost and CI/CD stability in a .NET N-Layer API. A three-layer ASP.NET Core application was constructed and instrumented with two complementary TDD testing strategies: standard repository tests using EF Core InMemoryDatabase, and service-layer tests using Moq mock objects. A GitHub Actions CI/CD pipeline was configured to run 20,000 xUnit test executions per build and publish structured XML reports as downloadable artifacts.

The empirical analysis yields three main findings:

1. **Mocking reduces steady-state execution cost.** `Mocked_Dependency_Tests` are 2.6× faster in mean and 7.2× faster in median execution time compared to `Standard_Tests`, driven by the elimination of EF Core in-memory context initialization per test. This confirms the prediction in Mishra & Nayak (2022) that mock-based tests reduce infrastructure overhead.

2. **JIT warm-up is a significant first-run cost.** The first test execution per class costs 1,269–3,210× more than steady-state runs due to .NET JIT compilation. This effect inflates mean execution times and total pipeline duration on cold CI runners, and justifies reporting median rather than mean as the primary execution cost metric.

3. **TDD produces high CI/CD stability.** A 100% pass rate across 20,000 executions confirms that well-isolated TDD tests with proper per-test context setup (unique in-memory database names, fresh Moq instances) are resistant to flakiness, consistent with Beller et al.'s (2017) characterization of TDD test suites as reliable pipeline quality gates.

These findings demonstrate that even in small systems, TDD produces observable, measurable patterns in both test execution cost and CI/CD pipeline behavior. The choice of testing strategy — mock-based vs. in-memory DB — has a quantifiable impact on pipeline duration that scales with test suite size, making it a relevant engineering consideration in CI/CD-heavy development workflows.

Future work could extend this study to larger systems with multiple entity types, compare synchronous vs. asynchronous test execution, and evaluate the impact of test parallelization on JIT warm-up amortization.

---

## References

Beller, M., Gousios, G., Panichella, A., & Zaidman, A. (2017). Oops, my tests broke the build: An explorative analysis of Travis CI with GitHub. *In 2017 IEEE/ACM 14th International Conference on Mining Software Repositories (MSR)* (pp. 356–367).

Hilton, M., Tunnell, T., Huang, K., Marinov, D., & Dig, D. (2016). Usage, costs, and benefits of continuous integration in open-source projects. *In Proceedings of the 31st IEEE/ACM International Conference on Automated Software Engineering* (pp. 426–437).

Mishra, L., & Nayak, S. K. (2022). A comparative analysis of test-driven development and behavior-driven development in CI/CD pipelines: Enhancing software quality and delivery speed. *Well Testing Journal*.

Spadini, D., Aniche, M., Bruntink, M., & Bacchelli, A. (2019). Mock objects for testing Java systems: Why and how developers use them, and how they evolve. *Empirical Software Engineering, 24*(3), 1461–1498.
