# ProductionManagementAI — Mô tả harness và quy trình phát triển bằng AI

- Phiên bản: 0.1 — mô tả ban đầu.
- Ngày: 2026-09-15.
- Ngôn ngữ của tài liệu này: tiếng Việt.
- Trạng thái: tổng hợp nội dung đã thống nhất và đề xuất cấu trúc để tiếp tục review; chưa phải bản triển khai hoàn chỉnh.
- Đối tượng: người quản lý dự án, BA, kiến trúc sư, lập trình viên, QA, người vận hành và người xây dựng harness.

## 1. Mục đích

Xây dựng một bộ harness để team có thể phát triển hệ thống bằng AI theo quy trình nhất quán, từ yêu cầu, thiết kế đến triển khai và kiểm thử. Claude và Codex cùng sử dụng nguồn hướng dẫn Markdown trong repository, thay vì phụ thuộc vào prompt riêng trong lịch sử chat của từng người.

ProductionManagementAI là dự án demo/MVP áp dụng harness này. Một vài màn hình quản lý sản xuất đơn giản sẽ được dùng để chứng minh quy trình phát triển đầy đủ, có tài liệu, code, kiểm thử, review và CI/CD.

Tài liệu này mô tả cách tổ chức và cách làm việc dự kiến. Việc có tài liệu không đồng nghĩa các skill, script, workflow CI/CD hoặc ứng dụng đã được tạo và chạy thành công.

## 2. Các quyết định đã chốt

| Hạng mục | Quyết định |
|---|---|
| Dự án demo | ProductionManagementAI |
| Frontend | Vite + TypeScript |
| Backend | .NET 10 |
| Database | PostgreSQL |
| Quản lý source và PR | GitHub |
| CI/CD giai đoạn đầu | GitHub Actions |
| Đóng gói và triển khai | Docker |
| Công cụ AI | Harness dùng chung cho Claude và Codex |
| Ngôn ngữ tài liệu dự án | Tiếng Anh mặc định, có thể chuyển sang tiếng Nhật |
| Tài liệu mô tả ban đầu | Tiếng Việt — chính là tài liệu này |
| Cách thực hiện | AI tạo plan, team review và duyệt, sau đó AI thực hiện trọn plan |
| Làm rõ trong quá trình chạy | Các bước có thể dừng để hỏi thêm khi cần |
| Định hướng cải tiến | AI có thể đề xuất nâng cấp skill, agent, tool và rule theo thay đổi của dự án |

Vite là công cụ phát triển/build, TypeScript là ngôn ngữ. Framework UI chưa được chốt; React đã được đề xuất nhưng chưa được chấp nhận. Không mặc định React hoặc Vue khi triển khai.

## 3. Thuật ngữ

| Thuật ngữ | Ý nghĩa trong dự án |
|---|---|
| Harness | Bộ hướng dẫn, trạng thái, mẫu, công cụ và kiểm chứng giúp AI thực hiện công việc có kiểm soát |
| BD — Basic Design / 基本設計 | Thiết kế cơ bản: nghiệp vụ, màn hình, luồng sử dụng, hành vi chính |
| DD — Detailed Design / 詳細設計 | Thiết kế chi tiết: xử lý, validation, trạng thái, API mapping và quy tắc triển khai |
| SA — System Architecture | Kiến trúc hệ thống và các quyết định kỹ thuật nền tảng |
| Skill | Hướng dẫn tái sử dụng cho một loại công việc, có đầu vào, cách làm, đầu ra và kiểm chứng |
| Rule | Quy tắc chung hoặc quy tắc theo công nghệ mà các bước phải tuân thủ |
| Workflow | Thứ tự công việc, điều kiện chuyển bước và cách xử lý ngoại lệ |
| Artifact | Sản phẩm của một bước, như tài liệu BD, migration, code hoặc báo cáo test |
| Quality gate | Điều kiện chất lượng cần đạt trước khi chuyển bước hoặc tích hợp thay đổi |
| Work item | Hồ sơ của một tính năng hoặc công việc, có ID và trạng thái riêng |
| Evidence | Bằng chứng thực hiện: lệnh đã chạy, kết quả, báo cáo, commit hoặc liên kết CI |
| Adapter | Cấu hình đầu vào riêng cho Claude/Codex để dẫn đến hướng dẫn chung |

## 4. Nguyên tắc thiết kế harness

### 4.1. Một nguồn hướng dẫn chung

Nội dung nghiệp vụ của harness nằm trong `ai/`. `AGENTS.md`, `CLAUDE.md`, `.claude/` và `.codex/` đóng vai trò đầu vào hoặc cấu hình riêng của công cụ.

Không duy trì hai bộ skill độc lập có cùng mục đích cho Claude và Codex. Khi một công cụ cần định dạng riêng, adapter tham chiếu hoặc được đồng bộ từ nguồn chung; cơ chế cụ thể sẽ được xác định trong bước triển khai.

### 4.2. Markdown làm hợp đồng làm việc

Người dùng bắt đầu bằng yêu cầu trong Markdown. AI đọc hướng dẫn và hồ sơ trong repository để tạo plan và thực hiện công việc. Thành viên khác hoặc công cụ AI khác phải có thể hiểu công việc mà không cần đọc toàn bộ chat trước đó.

Markdown mô tả cách làm, nhưng không tự thực thi build, test hay deploy. Script, công cụ và GitHub Actions thực hiện kiểm chứng. Vì vậy, harness gồm cả hướng dẫn và cơ chế kiểm tra có thể chạy được.

### 4.3. Plan được duyệt là phạm vi thực hiện

AI được thực hiện các bước nằm trong plan đã duyệt mà không cần xin duyệt lại từng bước. Điều kiện phải hỏi, phạm vi quyền hạn và giới hạn triển khai cần được ghi rõ trong plan và chính sách dự án.

### 4.4. Kết luận phải có bằng chứng

Không đánh dấu hoàn thành chỉ vì AI đã sinh đủ file. Artifact phải qua kiểm chứng phù hợp. Phân biệt rõ kết quả đã chạy thành công, kiểm tra chưa chạy, bước bị chặn và ngoại lệ đã được team chấp nhận.

### 4.5. Có thể tiếp tục từ trạng thái trong repository

Quyết định, câu hỏi đã trả lời, bước đang làm và bằng chứng được lưu vào hồ sơ công việc. Chuyển từ Claude sang Codex không có nghĩa bắt đầu lại hoặc tự động làm lại bước đã hoàn thành.

### 4.6. Chất lượng tương đương cần được đánh giá

Dùng chung Markdown giúp hai công cụ đi theo cùng quy trình, nhưng không bảo đảm đầu ra giống hệt hoặc chất lượng tự động tương đương. Cần dùng checklist và các tình huống đánh giá chung để kiểm tra chất lượng thực tế.

## 5. Cấu trúc repository đề xuất

Cấu trúc dưới đây là thiết kế v1 để review, chưa phải danh sách thư mục đã được triển khai.

```text
ProductionManagementAI/
├── AGENTS.md
├── CLAUDE.md
├── .claude/
├── .codex/
├── ai/
│   ├── README.md
│   ├── project.md
│   ├── policies.md
│   ├── workflows/
│   │   ├── project-bootstrap.md
│   │   ├── feature-delivery.md
│   │   ├── bug-fix.md
│   │   └── harness-improvement.md
│   ├── skills/
│   │   ├── planning/
│   │   ├── requirements/
│   │   ├── basic-design/
│   │   ├── screen-design/
│   │   ├── architecture/
│   │   ├── database-design/
│   │   ├── detailed-design/
│   │   ├── implementation/
│   │   ├── testing/
│   │   ├── pr-review/
│   │   └── ci-cd/
│   ├── rules/
│   ├── templates/
│   ├── checklists/
│   ├── evaluations/
│   └── improvements/
├── docs/
│   ├── vi/
│   │   └── 000-mo-ta-harness-va-quy-trinh-phat-trien-ai.md
│   ├── en/
│   │   ├── 000_requirements/
│   │   ├── 010_basic-design/
│   │   ├── 020_detailed-design/
│   │   ├── architecture/
│   │   ├── database/
│   │   └── testing/
│   └── ja/
├── work-items/
│   └── SCR-001/
│       ├── brief.md
│       ├── plan.md
│       ├── status.md
│       ├── decisions.md
│       └── evidence.md
├── src/
│   ├── frontend/
│   └── backend/
├── tests/
│   ├── frontend/
│   ├── backend/
│   ├── integration/
│   ├── system/
│   └── e2e/
├── scripts/
├── deploy/
│   ├── docker/
│   │   ├── frontend.Dockerfile
│   │   └── backend.Dockerfile
│   └── compose.yaml
├── demos/
└── .github/
    ├── workflows/
    │   ├── ci.yml
    │   └── cd.yml
    └── pull_request_template.md
```

### Trách nhiệm của các khu vực

| Khu vực | Nội dung |
|---|---|
| `ai/project.md` | Domain, stack, cấu trúc ứng dụng, lệnh chuẩn, môi trường và liên kết cần đọc |
| `ai/policies.md` | Quyền thực hiện, điểm phải hỏi, merge/deploy và ngoại lệ |
| `ai/workflows/` | Điều phối các bước và điều kiện hoàn thành |
| `ai/skills/` | Cách thực hiện từng loại công việc |
| `ai/rules/` | Quy tắc tài liệu, frontend, backend, DB, test và bảo mật liên quan |
| `ai/templates/` | Mẫu brief, plan, BD, DD, DB, test report và review |
| `ai/checklists/` | Tiêu chí kiểm tra artifact và đối chiếu giữa các artifact |
| `ai/evaluations/` | Tình huống mẫu để đánh giá harness và thay đổi của harness |
| `ai/improvements/` | Vấn đề quan sát được, đề xuất, đánh giá và lịch sử cải tiến |
| `docs/` | Tài liệu của hệ thống đang được xây dựng |
| `work-items/` | Trạng thái thực hiện và liên kết artifact của từng công việc |
| `scripts/` | Các lệnh kiểm chứng hoặc thao tác lặp lại được |
| `deploy/` | Dockerfile và cấu hình triển khai; không lưu secret |
| `demos/` | Kịch bản, dữ liệu, hướng dẫn và bằng chứng demo |

Trong MVP chưa cần tạo một hệ thống nhiều agent hoặc thư mục role riêng. Trách nhiệm được mô tả trong skill/workflow trước. Nếu bổ sung nhiều agent sau này, vẫn phải dùng cùng trạng thái và tiêu chí chất lượng.

Vị trí unit test cụ thể có thể được điều chỉnh theo công cụ đã chọn. Cần tránh lưu trùng một bộ test ở cả cạnh source và thư mục test chung.

## 6. Hai cấp quy trình

### 6.1. Khởi tạo dự án

Thực hiện trước khi triển khai tính năng đầu tiên:

1. Xác định phạm vi demo và các quyết định còn thiếu.
2. Tạo plan khởi tạo để team review.
3. Chốt SA nền tảng, quy ước source, dữ liệu và API.
4. Tạo harness nền tảng và cấu hình đầu vào cho hai công cụ AI.
5. Tạo skeleton ứng dụng, môi trường Docker và dữ liệu demo theo plan.
6. Thiết lập build, test, PR template và CI ban đầu.
7. Kiểm chứng quy trình bằng một tính năng xuyên suốt.

### 6.2. Phát triển một tính năng

```text
Yêu cầu và acceptance criteria
    → AI tạo plan
    → Team review và duyệt
    → Đối chiếu SA
    → BD và layout màn hình
    → DB, API contract và DD
    → Kiểm tra tính nhất quán thiết kế
    → Code và unit/integration test
    → System test và E2E
    → PR review và CI gates
    → Deploy demo và smoke test theo quyền đã duyệt
    → Tổng kết bằng chứng
```

SA nền tảng được xác định từ đầu. Khi tính năng đặt ra nhu cầu mới, AI đánh giá tác động đến SA và ghi quyết định thay đổi nếu có.

DB, API contract và DD có quan hệ qua lại. Có thể tạo bản nháp theo thứ tự thuận tiện, nhưng phải đối chiếu và thống nhất trước khi code phụ thuộc vào chúng.

CI và quy tắc PR review được thiết lập từ đầu và áp dụng xuyên suốt. Deploy chỉ được thực hiện theo môi trường và quyền đã xác định; việc duyệt plan phát triển không tự động cấp quyền merge hoặc deploy production.

## 7. Đầu vào, đầu ra và điều kiện hoàn thành từng bước

| Bước | Đầu vào | Đầu ra tối thiểu | Kiểm chứng trước khi chuyển bước |
|---|---|---|---|
| Yêu cầu | Mô tả nghiệp vụ, người dùng, vấn đề | Brief, phạm vi, acceptance criteria, câu hỏi mở | Có tiêu chí kiểm chứng được; phần chưa rõ được ghi nhận |
| Plan | Brief, context repo, policies | Các bước, phụ thuộc, artifact, kiểm tra, quyền và điểm dừng | Team duyệt và lưu dấu vết phiên bản được duyệt |
| SA | Yêu cầu và ràng buộc stack | Ranh giới thành phần, giao tiếp, cấu hình, quyết định kiến trúc | Đáp ứng phạm vi, không tự thêm công nghệ ngoài quyết định |
| BD | Yêu cầu và SA | Luồng nghiệp vụ, danh sách màn hình, layout, hành vi và quy tắc chính | Bao phủ acceptance criteria, luồng chính và ngoại lệ |
| DB | Nghiệp vụ, BD, dữ liệu hiện có | ERD, bảng/cột, khóa, constraint, index và định hướng migration | Dữ liệu hỗ trợ use case; ràng buộc và quan hệ nhất quán |
| DD và API | BD, SA, DB | Field, validation, trạng thái, xử lý, API request/response và lỗi | Khớp BD/DB/API; đủ rõ để triển khai và viết test |
| Code | Thiết kế đã kiểm tra | Frontend, backend, migration và cấu hình liên quan | Build, kiểm tra tĩnh và test phù hợp đạt yêu cầu |
| Kiểm thử hệ thống | Acceptance criteria, thiết kế, ứng dụng chạy được | System/E2E test và báo cáo | Có bằng chứng kết quả; lỗi và giới hạn được ghi rõ |
| PR review | Diff, tài liệu, evidence, CI | Nhận xét, sửa lỗi, tổng kết review | Không còn lỗi chặn chưa xử lý hoặc chưa được chấp nhận |
| Deploy | Image, cấu hình môi trường, quyền triển khai | Bản demo được triển khai và smoke test | Xác minh phiên bản chạy, kết nối và luồng smoke test |

Layout trong BD thể hiện bố cục và luồng thao tác. Thiết kế màn hình trong DD mô tả field, validation, loading/empty/error state, quyền nếu có và ánh xạ API. Đây là hai mức chi tiết của cùng màn hình.

## 8. Plan và cơ chế dừng hỏi

### 8.1. Nội dung bắt buộc của plan

- ID công việc, mục tiêu, phạm vi trong/ngoài công việc.
- Yêu cầu và acceptance criteria liên quan.
- Giả định, ràng buộc, câu hỏi còn mở.
- Các bước, phụ thuộc, skill áp dụng và artifact dự kiến.
- Lệnh hoặc phương pháp kiểm chứng từng bước.
- Rủi ro cụ thể và cách xử lý khi kiểm tra thất bại.
- Phạm vi thao tác: file, branch, PR, môi trường và các hành động được phép.
- Điều kiện phải hỏi, điều kiện cần cập nhật plan để review lại.
- Phiên bản plan và dấu vết duyệt của team.

### 8.2. Khi nào AI tiếp tục tự động?

AI tiếp tục khi công việc nằm trong plan, thông tin đủ rõ và các quality gate liên quan đạt yêu cầu. Các lựa chọn triển khai thông thường có thể do AI quyết định trong phạm vi quy tắc đã thống nhất và được ghi lại khi cần.

### 8.3. Khi nào AI dừng hỏi?

- Yêu cầu hoặc tài liệu mâu thuẫn, không thể xác định hành vi đúng.
- Thiếu quyết định ảnh hưởng nghiệp vụ, dữ liệu hoặc acceptance criteria.
- Phát sinh thay đổi đáng kể về phạm vi, kiến trúc hoặc công nghệ.
- Cần quyền, cấu hình hoặc môi trường chưa được cấp trong plan.
- Thao tác có tác động phá hủy dữ liệu hoặc triển khai vượt phạm vi đã duyệt.
- Một trở ngại không thể xử lý an toàn trong phạm vi được giao.

Khi hỏi, AI nêu rõ vấn đề, tác động, lựa chọn và đề xuất. Ghi câu trả lời vào `decisions.md`, cập nhật trạng thái và tiếp tục từ bước bị chặn. Có thể tiếp tục phần độc lập không phụ thuộc câu trả lời; không tự xem sự im lặng là đồng ý.

Không phải câu hỏi nào cũng cần duyệt lại toàn bộ plan. Chỉ review lại phần bị ảnh hưởng khi câu trả lời thay đổi phạm vi hoặc quyết định đáng kể.

## 9. Hồ sơ công việc và chuyển giữa Claude/Codex

| File | Nội dung cần lưu |
|---|---|
| `brief.md` | Yêu cầu, phạm vi, acceptance criteria và nguồn tham chiếu |
| `plan.md` | Plan hiện tại và thông tin review/duyệt |
| `status.md` | Bước hiện tại, bước hoàn thành, blocker và hành động tiếp theo |
| `decisions.md` | Câu hỏi, câu trả lời, quyết định, lý do và tác động |
| `evidence.md` | Liên kết artifact, yêu cầu–thiết kế–test, kết quả lệnh, PR và CI |

Trạng thái gợi ý: `draft`, `awaiting-plan-review`, `ready`, `in-progress`, `blocked`, `in-review`, `done`. Đây là đề xuất, cần được chuẩn hóa khi triển khai harness.

Agent tiếp nhận công việc cần:

1. Đọc entry point, `ai/project.md`, `ai/policies.md` và workflow phù hợp.
2. Đọc hồ sơ work item và xác định plan đã được duyệt.
3. Kiểm tra trạng thái repository, artifact và bằng chứng hiện có.
4. Xác định bước tiếp theo; không ghi đè thay đổi chưa hiểu của người khác.
5. Thực hiện, kiểm chứng và cập nhật hồ sơ sau mỗi mốc có ý nghĩa.

`done` chỉ được dùng khi phạm vi plan đã hoàn tất, các kiểm tra bắt buộc có kết quả và mọi giới hạn còn lại đã được xử lý theo chính sách. Nếu deploy nằm ngoài phạm vi, báo cáo rõ điều đó; không mô tả hệ thống là đã deploy.

## 10. Cấu trúc chuẩn của một skill

Mỗi skill cần có:

1. Mục đích và tình huống sử dụng.
2. Đầu vào bắt buộc, đầu vào tùy chọn và thứ tự nguồn tham chiếu.
3. Các bước thực hiện và quy tắc áp dụng.
4. Tool/script cần dùng và điều kiện môi trường.
5. Artifact đầu ra, template, quy ước ID và nơi lưu.
6. Checklist và cách kiểm chứng có thể lặp lại.
7. Điều kiện dừng hỏi và xử lý khi thất bại.
8. Cách cập nhật work item và bàn giao bước tiếp theo.

Định dạng nhận diện skill của từng công cụ sẽ được kiểm tra khi triển khai adapter. Không giả định rằng mọi thư mục Markdown tự động được cả Claude và Codex khám phá.

## 11. Chiến lược kiểm thử

| Loại | Mục tiêu | Ví dụ cho lệnh sản xuất |
|---|---|---|
| Unit | Kiểm tra một đơn vị logic độc lập | Validation số lượng hoặc quy tắc chuyển trạng thái |
| Integration | Kiểm tra thành phần phối hợp thực tế | API lưu và đọc dữ liệu qua PostgreSQL |
| System | Kiểm tra hệ thống theo yêu cầu nghiệp vụ | Tạo lệnh hợp lệ; từ chối dữ liệu vi phạm quy tắc |
| E2E | Kiểm tra hành trình xuyên suốt từ UI | Người dùng nhập form, lưu và nhìn thấy lệnh trong danh sách |
| Smoke | Kiểm tra nhanh sau khởi động/deploy | Frontend truy cập được, backend sẵn sàng, luồng cơ bản hoạt động |

System test mô tả phạm vi kiểm tra yêu cầu ở cấp hệ thống. E2E mô tả cách kiểm tra xuyên qua các lớp từ góc nhìn người dùng. Một E2E test có thể là bằng chứng cho system test; không cần nhân đôi test chỉ để có đủ tên nhóm.

Test cần truy về acceptance criteria và các rủi ro chính. Sử dụng dữ liệu test có thể thiết lập lại, môi trường tách biệt và kết quả rõ ràng. Framework test cụ thể, coverage threshold và các gate bắt buộc chưa được chốt.

## 12. GitHub, PR review và Docker deployment

### 12.1. Luồng tích hợp đề xuất

1. Làm việc trên branch theo quy ước sẽ được chốt.
2. Tạo thay đổi trong phạm vi plan, đồng bộ tài liệu và test.
3. Chạy kiểm tra cục bộ phù hợp.
4. Tạo PR với mục tiêu, thay đổi, bằng chứng, giới hạn và tác động DB/deploy.
5. GitHub Actions chạy các gate đã cấu hình.
6. AI review diff theo yêu cầu, thiết kế, tính đúng đắn và khả năng hồi quy; sửa và kiểm tra lại khi cần.
7. Người có trách nhiệm review/merge theo chính sách của team.
8. CD build/publish image và deploy theo trigger, môi trường và quyền được xác định.
9. Chạy smoke test và ghi kết quả.

AI review là một lớp kiểm tra. AI không tự coi review của mình là quyền merge hoặc quyền bỏ qua kiểm tra thất bại.

### 12.2. Phạm vi CI/CD dự kiến

- CI: kiểm tra tài liệu/harness khi phù hợp, frontend/backend build, lint hoặc static checks, unit và integration test.
- System/E2E: chạy với môi trường ứng dụng và PostgreSQL đã chuẩn bị; trigger chính xác sẽ được chốt dựa trên thời gian chạy và nhu cầu team.
- CD: build image, publish vào registry, triển khai Docker và smoke test.
- Cấu hình môi trường và secret được quản lý ngoài source; tài liệu chỉ lưu tên biến và cách thiết lập.
- Migration và rollback cần có phương án trước khi bật deploy tự động. Không mặc định mọi migration đều có thể rollback mà không ảnh hưởng dữ liệu.

Docker là cách đóng gói/triển khai đã chốt. Docker Compose là đề xuất phù hợp cho môi trường demo, chưa xác định host, registry, domain hoặc nền tảng production.

## 13. Ngôn ngữ và truy vết tài liệu

Tài liệu dự án mới sử dụng tiếng Anh mặc định. Có thể tạo bản tiếng Nhật khi cần; tài liệu mô tả ban đầu này dùng tiếng Việt theo yêu cầu.

- Dùng ID ổn định, ví dụ `REQ-001`, `BD-SCR-001`, `DD-SCR-001`, `TC-SCR-001-01`.
- ID không thay đổi theo ngôn ngữ.
- Nếu duy trì bản dịch, ghi rõ tài liệu nguồn và phiên bản/commit nguồn.
- Khi nguồn thay đổi, đánh dấu bản dịch cần cập nhật; không coi hai bản khác nội dung đều là nguồn chính.
- Không tạo bản dịch hàng loạt khi chưa có nhu cầu.
- `010_basic-design` tương ứng `010_基本設計`; `020_detailed-design` tương ứng `020_詳細設計`.

Ví dụ truy vết: yêu cầu tạo lệnh sản xuất → BD màn hình → DD xử lý lưu → bảng/API liên quan → code → test case → kết quả CI. Liên kết này được lưu ở artifact hoặc `evidence.md`, không cần sao chép toàn bộ nội dung vào nhiều nơi.

## 14. Phạm vi demo và bốn video

### 14.1. Phạm vi màn hình đề xuất, chưa chốt

- Danh mục sản phẩm.
- Danh sách lệnh sản xuất.
- Tạo/sửa lệnh sản xuất.

Đề xuất chọn màn hình A là **Tạo/sửa lệnh sản xuất**, vì có thể minh họa layout, validation, API, DB, code và test trong một luồng nhỏ. Các field, trạng thái, quyền và quy tắc nghiệp vụ sẽ được định nghĩa trong brief; hiện chưa được chốt.

### 14.2. Bốn video theo yêu cầu

| Video | Nội dung | Artifact và bằng chứng cần thể hiện |
|---|---|---|
| 1 | Thiết lập skill và prompt để tạo BD/layout màn hình A | Tài liệu tương ứng `010_基本設計`, layout và checklist |
| 2 | Thiết lập skill và prompt để tạo DD màn hình A | Tài liệu tương ứng `020_詳細設計`, liên kết BD và các điểm cần đối chiếu |
| 3 | Thiết lập skill và prompt để tạo thiết kế DB cho màn hình A | ERD, schema, constraint và mapping với DD |
| 4 | Thiết lập skill và prompt để tạo code cho màn hình A | Ứng dụng chạy được, test, PR và CI evidence |

Mỗi kịch bản cần mô tả điểm bắt đầu, file đầu vào, skill được dùng, prompt ngắn, plan được review, thao tác chạy, artifact và cách kiểm chứng. Video 4 hoặc phần phụ lục demo cần thể hiện đường đi tới deploy và smoke test để chứng minh quy trình đầy đủ.

Thứ tự video là cách trình bày. Nếu DD được tạo trước DB trong video, DD vẫn là bản cần đối chiếu cho đến khi DB/API được thống nhất. Không chuyển sang code phụ thuộc thiết kế chưa nhất quán.

Mục tiêu là chứng minh cùng quy trình có thể dùng với Claude và Codex, bao gồm khả năng tiếp tục từ work item. Chưa chốt cách phân bổ hai công cụ giữa các video, công cụ quay hoặc việc tự động tạo file video thực tế.

## 15. Cải tiến skill, agent, tool và rule

### 15.1. Mục tiêu

Harness cần thay đổi theo nghiệp vụ, stack, công cụ và lỗi thực tế. AI có thể phát hiện lỗ hổng hướng dẫn và tạo đề xuất cải tiến có bằng chứng.

### 15.2. Vòng lặp đề xuất

```text
Lỗi thực tế / nhận xét review / thay đổi dự án
    → Ghi vấn đề và nguyên nhân
    → Đề xuất thay đổi harness
    → Tạo patch trên branch/PR
    → Chạy evaluation và kiểm tra hồi quy liên quan
    → Team review
    → Chấp nhận phiên bản mới hoặc từ chối/rollback
```

Mỗi đề xuất ghi: vấn đề, bằng chứng, file bị ảnh hưởng, hành vi trước/sau mong muốn, đánh giá, rủi ro và cách hoàn tác.

### 15.3. Mức tự động hóa đề xuất cho MVP

AI tự phát hiện và tạo patch/PR cải tiến; team duyệt trước khi áp dụng vào hướng dẫn chung. Đây là cơ chế được đề xuất để hiện thực hóa nhu cầu tự cải tiến, cần chốt trong policies khi triển khai.

Không cho phép thay đổi harness âm thầm để hợp thức hóa một lần chạy thất bại. Việc sửa quality gate, quyền thao tác hoặc điều kiện review phải được nêu rõ và đánh giá. Thay đổi tool hoặc thêm dịch vụ cũng cần xét quyền truy cập và cấu hình liên quan.

## 16. Trách nhiệm của team và AI

| Chủ thể | Trách nhiệm |
|---|---|
| Người phụ trách nghiệp vụ | Cung cấp mục tiêu, xác nhận hành vi và acceptance criteria |
| Người review plan | Kiểm tra phạm vi, cách làm, kiểm chứng và quyền thực hiện |
| AI thực hiện | Đọc context, tạo plan, tạo artifact, chạy kiểm tra, lưu tiến độ và hỏi khi cần |
| Người phụ trách kỹ thuật | Review kiến trúc, tiêu chuẩn và quyết định có tác động lớn |
| QA/người nghiệm thu | Đánh giá mức bao phủ yêu cầu và kết quả kiểm thử |
| Người có quyền merge/deploy | Thực hiện hoặc cho phép hành động theo chính sách đã chốt |
| Người duy trì harness | Review cải tiến và quản lý phiên bản hướng dẫn chung |

Một người có thể đảm nhiệm nhiều trách nhiệm trong MVP. Không yêu cầu phải có một agent riêng cho mỗi vai trò.

## 17. Ví dụ cách làm việc hằng ngày

1. Thành viên tạo `work-items/SCR-001/brief.md` với yêu cầu và tiêu chí nghiệm thu.
2. Yêu cầu Claude hoặc Codex đọc harness và lập plan cho work item đó.
3. AI đọc context, nêu câu hỏi cần thiết và tạo `plan.md`.
4. Team review, điều chỉnh và xác nhận phiên bản plan được duyệt.
5. AI thực hiện thiết kế, code và test theo plan, cập nhật hồ sơ từng mốc.
6. Nếu thiếu quy tắc nghiệp vụ, AI ghi blocker và hỏi; câu trả lời được lưu vào `decisions.md`.
7. Nếu đổi công cụ AI, agent mới đọc hồ sơ và tiếp tục từ bước phù hợp.
8. AI tạo PR, tổng hợp bằng chứng và xử lý nhận xét trong phạm vi được giao.
9. Merge/deploy theo chính sách; kết quả được cập nhật vào hồ sơ.
10. Vấn đề lặp lại được chuyển thành đề xuất cải tiến harness.

Ví dụ yêu cầu sau khi harness đã được triển khai:

> Đọc hướng dẫn dự án và work item SCR-001. Tạo plan để team review, chỉ rõ đầu ra, kiểm chứng và các quyết định còn thiếu.

Sau khi duyệt:

> Thực hiện plan đã duyệt của SCR-001. Cập nhật trạng thái và bằng chứng trong repo. Nếu gặp điều kiện dừng đã quy định, ghi rõ vấn đề và hỏi để tiếp tục.

Đây là ví dụ cách tương tác, không phải lệnh đã được chạy hoặc sự phê duyệt triển khai hiện tại.

## 18. Lộ trình triển khai đề xuất

### Giai đoạn 1 — Harness nền tảng

- Chốt framework frontend, cấu trúc backend và các quyết định nền tảng còn thiếu.
- Hoàn thiện workflow, policy, template plan và các skill tối thiểu.
- Tạo entry point cho Claude/Codex và kiểm tra khả năng đọc hướng dẫn.
- Xác định lệnh kiểm chứng, hồ sơ công việc và cách ghi nhận duyệt plan.

### Giai đoạn 2 — Một tính năng xuyên suốt

- Chốt màn hình A và acceptance criteria.
- Chạy đầy đủ yêu cầu → plan → thiết kế → code → test → PR.
- Thiết lập Docker, GitHub Actions và deploy demo trong phạm vi đã duyệt.
- Thu thập bằng chứng, điều chỉnh harness từ vấn đề quan sát được.

### Giai đoạn 3 — Kiểm chứng khả năng tái sử dụng

- Thử cùng workflow với Claude và Codex, kể cả chuyển công cụ giữa chừng.
- Hoàn thiện bốn kịch bản video và dữ liệu demo có thể thiết lập lại.
- Xây evaluation ban đầu và thử một PR cải tiến harness.
- Bổ sung các màn hình còn lại khi quy trình đầu tiên ổn định.

## 19. Tiêu chí thành công đề xuất cho MVP

- Thành viên mới hiểu cách bắt đầu công việc bằng cách đọc repository.
- Claude và Codex đều có thể lập plan theo cùng template và tiếp tục từ trạng thái lưu trong repo.
- Một màn hình có đầy đủ truy vết từ yêu cầu đến BD, DD, DB, code và test.
- Plan được review trước khi thực hiện; câu hỏi và thay đổi quyết định có lịch sử.
- Unit, integration, system và E2E có phạm vi rõ ràng và bằng chứng phù hợp.
- PR thể hiện thay đổi, kiểm chứng và giới hạn; GitHub Actions thực thi các gate đã chốt.
- Ứng dụng demo chạy bằng Docker và có bằng chứng smoke test ở môi trường được duyệt.
- Có bốn kịch bản video tái hiện được các bước yêu cầu.
- Có cơ chế đề xuất và đánh giá thay đổi harness trước khi áp dụng.

## 20. Những điểm còn cần quyết định

| Chủ đề | Trạng thái / quyết định cần có |
|---|---|
| Framework frontend | Chưa chốt; React là đề xuất trước đó |
| UI library và thiết kế trực quan | Chưa chốt |
| Cấu trúc backend, ORM và migration | Chưa chốt |
| Phiên bản PostgreSQL | Chưa chốt |
| Framework test và ngưỡng quality gate | Chưa chốt |
| Nghiệp vụ màn hình A | Tạo/sửa lệnh sản xuất là đề xuất |
| Authentication và phân quyền | Cần xác định có nằm trong MVP hay không |
| GitHub repository và branch policy | Chưa chốt chi tiết |
| Cách ghi nhận duyệt plan | Cần chọn quy ước file, PR hoặc cơ chế phù hợp |
| Quyền tạo PR, merge và deploy | Cần ghi rõ trong policies và plan |
| Docker registry, host deploy và môi trường | Chưa chốt |
| Secret, migration, backup và rollback | Xác định theo môi trường deploy thực tế |
| Chính sách bản dịch tiếng Nhật | Chỉ tạo khi có nhu cầu; cần chốt cách kiểm tra đồng bộ |
| Cách quay bốn video | Chưa chốt công cụ và phân bổ Claude/Codex |
| Tự cải tiến harness | Đề xuất AI tạo PR, team duyệt; cần chuẩn hóa thành policy |

## 21. Phạm vi của bản mô tả này

Đây là tài liệu khởi đầu để mọi người thống nhất mục tiêu, cấu trúc và cách cộng tác trước khi tạo scaffold hoặc code. Bước tiếp theo là review tài liệu, chốt các quyết định cần thiết cho giai đoạn 1 và để AI tạo plan triển khai cụ thể.

Việc tạo tài liệu này không đồng nghĩa đã phê duyệt plan triển khai ứng dụng, đã chọn framework frontend hoặc đã cấp quyền merge/deploy.
