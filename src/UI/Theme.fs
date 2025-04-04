// UI/Theme.fs
module App.UI.Theme

// 色
module Colors =
    let primary = "blue"
    let secondary = "gray"
    let success = "green"
    let warning = "yellow"
    let error = "red"
    let info = "blue"

// コンポーネントクラス
module Classes =
    let button (variant: string) (size: string) =
        let baseClasses = "rounded transition-colors"

        let variantClasses =
            match variant with
            | "primary" -> $"bg-{Colors.primary}-500 text-white hover:bg-{Colors.primary}-600"
            | "secondary" -> $"bg-{Colors.secondary}-200 text-{Colors.secondary}-800 hover:bg-{Colors.secondary}-300"
            | "outline" -> $"border border-{Colors.secondary}-300 hover:bg-{Colors.secondary}-100"
            | "danger" -> $"bg-{Colors.error}-500 text-white hover:bg-{Colors.error}-600"
            | "success" -> $"bg-{Colors.success}-500 text-white hover:bg-{Colors.success}-600"
            | _ -> $"bg-{Colors.secondary}-200 hover:bg-{Colors.secondary}-300"

        let sizeClasses =
            match size with
            | "sm" -> "px-2 py-1 text-sm"
            | "lg" -> "px-6 py-3 text-lg"
            | _ -> "px-4 py-2"

        $"{baseClasses} {variantClasses} {sizeClasses}"

    let card = "bg-white rounded-lg shadow-md p-4"

    let input (hasError: bool) =
        let baseClasses = "w-full p-2 border rounded"

        if hasError then
            $"{baseClasses} border-{Colors.error}-500"
        else
            $"{baseClasses} border-{Colors.secondary}-300"

    let table = "min-w-full divide-y divide-gray-200"

    let tableHeader =
        "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"

    let tableRow (isSelected: bool) = if isSelected then "bg-blue-50" else ""

    let tableCell = "px-6 py-4 whitespace-nowrap"

    let badge (type': string) =
        match type' with
        | "success" ->
            $"px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-{Colors.success}-100 text-{Colors.success}-800"
        | "warning" ->
            $"px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-{Colors.warning}-100 text-{Colors.warning}-800"
        | "error" ->
            $"px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-{Colors.error}-100 text-{Colors.error}-800"
        | "info" ->
            $"px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-{Colors.info}-100 text-{Colors.info}-800"
        | _ ->
            $"px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-{Colors.secondary}-100 text-{Colors.secondary}-800"

    let pageHeader = "flex justify-between items-center mb-6"

    let container = "container mx-auto p-5"

    let tab (isActive: bool) =
        if isActive then
            "px-5 py-2.5 bg-white border border-gray-200 border-b-white -mb-px font-medium"
        else
            "px-5 py-2.5 bg-gray-100 border border-gray-200 hover:bg-gray-200 transition-colors"

    let tabContainer = "flex flex-wrap border-b border-gray-200 mb-5"

    let modalOverlay =
        "fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"

    let modal =
        "bg-white rounded-lg shadow-xl p-6 max-w-lg w-full max-h-[90vh] overflow-auto"

    let formGroup = "form-group mb-4"

    let formLabel = "block text-sm font-medium text-gray-700 mb-1"

    let formError = "text-red-500 text-sm mt-1"

    let infoText = "text-gray-500"

    let loadingSpinner =
        "animate-spin h-8 w-8 border-4 border-blue-500 rounded-full border-t-transparent mx-auto mb-4"

    let emptyState = "p-8 text-center"

    let errorState = "p-8 text-center bg-red-50 rounded-lg"

// アイコン定義
module Icons =
    let iconClasses = "inline-block align-text-bottom"

    let spinner = "animate-spin h-5 w-5 text-white"

    // 基本アイコン
    let user = "👤"
    let users = "👥"
    let edit = "✏️"
    let delete = "🗑️"
    let view = "👁️"
    let add = "➕"
    let close = "✖️"
    let search = "🔍"
    let settings = "⚙️"
    let home = "🏠"
    let list = "📋"
    let chart = "📊"
    let dashboard = "📊"
    let product = "📦"
    let download = "⬇️"
    let upload = "⬆️"
    let check = "✓"
    let warning = "⚠️"
    let info = "ℹ️"
    let error = "❌"
    let success = "✅"

// レスポンシブ設計
module Responsive =
    let hiddenOnMobile = "hidden md:block"
    let hiddenOnDesktop = "md:hidden"
    let colSpan1 = "col-span-1"
    let colSpan2 = "col-span-2"
    let colSpan3 = "col-span-3"
    let colSpan4 = "col-span-4"
    let colSpanFull = "col-span-full"
    let colSpanResponsive = "lg:col-span-2"

    let flexCol = "flex flex-col"
    let flexRow = "flex flex-row"
    let flexColToRow = "flex flex-col md:flex-row"

    let gridCols1 = "grid-cols-1"
    let gridCols2 = "grid-cols-1 md:grid-cols-2"
    let gridCols3 = "grid-cols-1 md:grid-cols-2 lg:grid-cols-3"
    let gridCols4 = "grid-cols-1 md:grid-cols-2 lg:grid-cols-4"
