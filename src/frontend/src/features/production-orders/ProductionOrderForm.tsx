import { useEffect, useRef, useState, type FormEvent, type ReactNode } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useNavigationGuard } from '../../lib/navigationGuard'
import { ApiError } from '../../lib/apiClient'
import { createOrder, updateOrder } from './api'
import { DiscardChangesDialog } from './DiscardChangesDialog'
import { MessageBanner, type Banner } from './MessageBanner'
import { labels, message, statusLabels } from './messages'
import type { Product, ProductionOrder, ProductionOrderStatus } from './types'
import {
  MAX_NOTES,
  codePointLength,
  localToday,
  validateDueDate,
  validateNotes,
  validateProduct,
  validateQuantity,
} from './validation'
import { formatTimestamp } from '../../lib/format'

type Field = 'productId' | 'quantity' | 'dueDate' | 'notes'
const FIELDS: Field[] = ['productId', 'quantity', 'dueDate', 'notes']

interface FormValues {
  productId: string
  quantity: string
  dueDate: string
  status: ProductionOrderStatus
  notes: string
}

interface Props {
  mode: 'create' | 'edit'
  order: ProductionOrder | null
  products: Product[]
  initialBanner: Banner | null
  onCreated: (order: ProductionOrder) => void
  onReload: () => void
  onForbidden: () => void
  onNotFound: () => void
}

function toValues(order: ProductionOrder | null): FormValues {
  return order
    ? {
        productId: order.productId,
        quantity: String(order.quantity),
        dueDate: order.dueDate,
        status: order.status,
        notes: order.notes ?? '',
      }
    : { productId: '', quantity: '', dueDate: '', status: 'Draft', notes: '' }
}

function isSame(a: FormValues, b: FormValues): boolean {
  return (
    a.productId === b.productId &&
    a.quantity.trim() === b.quantity.trim() &&
    a.dueDate === b.dueDate &&
    a.status === b.status &&
    a.notes.trim() === b.notes.trim()
  )
}

const inputClass =
  'w-full rounded border border-gray-300 bg-white px-3 py-2 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none aria-[invalid=true]:border-red-600 disabled:bg-gray-100 disabled:text-gray-500'
const readOnlyClass =
  'w-full rounded border border-dashed border-gray-300 bg-gray-50 px-3 py-2 text-gray-600 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none'

// 001_DD module 8 / 001_DD-SPD §2–§3: controlled form for 001_BD items 6–16.
export function ProductionOrderForm({
  mode,
  order,
  products,
  initialBanner,
  onCreated,
  onReload,
  onForbidden,
  onNotFound,
}: Props) {
  const navigate = useNavigate()
  const [current, setCurrent] = useState(order)
  const [initial, setInitial] = useState(() => toValues(order))
  const [values, setValues] = useState(initial)
  const [errors, setErrors] = useState<Partial<Record<Field, string>>>({})
  const [banner, setBanner] = useState(initialBanner)
  const [saving, setSaving] = useState(false)
  const [confirmingDiscard, setConfirmingDiscard] = useState(false)

  const productInput = useRef<HTMLSelectElement & HTMLInputElement>(null)
  const quantityInput = useRef<HTMLInputElement>(null)
  const dueDateInput = useRef<HTMLInputElement>(null)
  const notesInput = useRef<HTMLTextAreaElement>(null)
  const cancelButton = useRef<HTMLButtonElement>(null)

  const locked = current !== null && !current.isProductQuantityEditable
  const statusOptions: ProductionOrderStatus[] = current ? [current.status, ...current.allowedNextStatuses] : ['Draft']
  const statusFixed = current !== null && current.allowedNextStatuses.length === 0
  const isDirty = !isSame(values, initial)

  // WI-004 DEC-022: any in-app link that would leave an edited form asks first, as Cancel does (001_BD E-07a).
  const guard = useNavigationGuard()
  const { pathname } = useLocation()
  const pendingTo = useRef<string | null>(null)
  const returnFocus = useRef<HTMLElement | null>(null)
  useEffect(
    () =>
      guard.register((to) => {
        if (!isDirty) {
          return false
        }
        pendingTo.current = to
        returnFocus.current = document.activeElement instanceof HTMLElement ? document.activeElement : null
        setConfirmingDiscard(true)
        return true
      }),
    [guard, isDirty],
  )
  const notesLength = codePointLength(values.notes)
  const selectedProduct = products.find((p) => p.id === values.productId)

  function validateField(field: Field): string | null {
    switch (field) {
      case 'productId':
        return locked ? null : validateProduct(values.productId)
      case 'quantity':
        return locked ? null : validateQuantity(values.quantity)
      case 'dueDate':
        return validateDueDate(values.dueDate, mode === 'edit' ? initial.dueDate : null, localToday())
      case 'notes':
        return validateNotes(values.notes)
    }
  }

  function setField<K extends keyof FormValues>(field: K, value: FormValues[K]) {
    setValues((previous) => ({ ...previous, [field]: value }))
  }

  function handleBlur(field: Field) {
    const error = validateField(field)
    setErrors((previous) => ({ ...previous, [field]: error ?? undefined }))
  }

  function showFieldErrors(next: Partial<Record<Field, string>>) {
    setErrors(next)
    const first = FIELDS.find((field) => next[field])
    const target = { productId: productInput, quantity: quantityInput, dueDate: dueDateInput, notes: notesInput }
    if (first) {
      target[first].current?.focus()
    }
  }

  function handleError(error: unknown) {
    if (!(error instanceof ApiError)) {
      setBanner({ kind: 'error', text: message('MSG-E013') })
      return
    }

    switch (error.status) {
      case 400: {
        const next: Partial<Record<Field, string>> = {}
        let other: string | null = null
        for (const [key, ids] of Object.entries(error.problem?.errors ?? {})) {
          if ((FIELDS as string[]).includes(key)) {
            next[key as Field] = ids[0]
          } else {
            other ??= ids[0]
          }
        }
        showFieldErrors(next)
        if (other) {
          setBanner({ kind: 'error', text: message(other) })
        }
        return
      }
      case 401:
        return // the apiClient's handler clears the session; ProtectedRoute redirects to /login
      case 403:
        onForbidden()
        return
      case 404:
        onNotFound()
        return
      case 409:
        setBanner({ kind: 'error', text: message('MSG-E009'), onReload })
        return
      case 422:
        setBanner({ kind: 'error', text: message(error.problem?.code ?? 'MSG-E013') })
        return
      default:
        setBanner({ kind: 'error', text: message('MSG-E013') })
    }
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()

    const next: Partial<Record<Field, string>> = {}
    for (const field of FIELDS) {
      const error = validateField(field)
      if (error) {
        next[field] = error
      }
    }
    if (Object.keys(next).length > 0) {
      showFieldErrors(next)
      return
    }

    setBanner(null)
    setSaving(true)
    const body = {
      productId: values.productId,
      quantity: Number(values.quantity.trim()),
      dueDate: values.dueDate,
      notes: values.notes.trim() === '' ? null : values.notes.trim(),
    }

    try {
      if (mode === 'create' || current === null) {
        onCreated(await createOrder(body))
        return
      }

      const saved = await updateOrder(current.id, { ...body, status: values.status, version: current.version })
      const savedValues = toValues(saved)
      setCurrent(saved)
      setInitial(savedValues)
      setValues(savedValues)
      setErrors({})
      setBanner({ kind: 'success', text: message('MSG-I002', { orderNumber: saved.orderNumber }) })
    } catch (error) {
      handleError(error)
    } finally {
      setSaving(false)
    }
  }

  function handleCancel() {
    if (isDirty) {
      setConfirmingDiscard(true)
    } else {
      navigate('/production-orders')
    }
  }

  function describedBy(field: Field, ...extra: string[]) {
    return [errors[field] ? `${field}-error` : null, ...extra].filter(Boolean).join(' ') || undefined
  }

  return (
    <>
      <MessageBanner banner={banner} />
      <form noValidate onSubmit={(event) => void handleSubmit(event)} className="grid gap-4">
        <div className="grid gap-4 rounded-lg border border-gray-200 bg-white p-4 sm:p-6">
          <p className="text-xs text-gray-500">
            {labels.order.requiredLegend}
          </p>

          <Row label={labels.order.orderNumber} labelId="orderNumber-label">
            <p aria-labelledby="orderNumber-label" className={current ? 'text-gray-900' : 'text-gray-500 italic'}>
              {current ? current.orderNumber : labels.order.assignedOnSave}
            </p>
          </Row>

          <Row label={labels.order.status} htmlFor={mode === 'edit' ? 'status' : undefined} labelId="status-label">
            {mode === 'edit' ? (
              <select
                id="status"
                value={values.status}
                disabled={statusFixed || saving}
                onChange={(event) => setField('status', event.target.value as ProductionOrderStatus)}
                className={inputClass}
              >
                {statusOptions.map((status) => (
                  <option key={status} value={status}>
                    {statusLabels[status]}
                  </option>
                ))}
              </select>
            ) : (
              <p aria-labelledby="status-label" className="text-gray-900">
                {statusLabels.Draft}
              </p>
            )}
          </Row>

          <Row label={labels.order.product} htmlFor="productId" required>
            {locked ? (
              <input
                id="productId"
                ref={productInput}
                readOnly
                aria-readonly="true"
                aria-required="true"
                aria-describedby="productId-lock"
                value={selectedProduct ? `${selectedProduct.sku} — ${selectedProduct.name}` : values.productId}
                className={readOnlyClass}
              />
            ) : (
              <select
                id="productId"
                ref={productInput}
                value={values.productId}
                aria-required="true"
                aria-invalid={errors.productId ? true : undefined}
                aria-describedby={describedBy('productId')}
                disabled={products.length === 0}
                onChange={(event) => setField('productId', event.target.value)}
                onBlur={() => handleBlur('productId')}
                className={inputClass}
              >
                <option value="">{products.length === 0 ? message('MSG-E014') : labels.order.selectProduct}</option>
                {products.map((product) => (
                  <option key={product.id} value={product.id}>
                    {product.sku} — {product.name}
                  </option>
                ))}
              </select>
            )}
            {locked && <LockHint id="productId-lock" />}
            <FieldError field="productId" error={errors.productId} />
          </Row>

          <div className="grid gap-4 sm:grid-cols-2">
            <Row label={labels.order.quantity} htmlFor="quantity" required>
              <input
                id="quantity"
                ref={quantityInput}
                type="text"
                inputMode="numeric"
                maxLength={9}
                readOnly={locked}
                aria-required="true"
                aria-readonly={locked ? 'true' : undefined}
                aria-invalid={errors.quantity ? true : undefined}
                aria-describedby={describedBy('quantity', ...(locked ? ['productId-lock'] : []))}
                value={values.quantity}
                onChange={(event) => setField('quantity', event.target.value)}
                onBlur={() => handleBlur('quantity')}
                className={locked ? readOnlyClass : inputClass}
              />
              <FieldError field="quantity" error={errors.quantity} />
            </Row>

            <Row label={labels.order.dueDate} htmlFor="dueDate" required>
              <input
                id="dueDate"
                ref={dueDateInput}
                type="date"
                aria-required="true"
                aria-invalid={errors.dueDate ? true : undefined}
                aria-describedby={describedBy('dueDate')}
                value={values.dueDate}
                onChange={(event) => setField('dueDate', event.target.value)}
                onBlur={() => handleBlur('dueDate')}
                className={inputClass}
              />
              <FieldError field="dueDate" error={errors.dueDate} />
            </Row>
          </div>

          <Row label={labels.order.notes} htmlFor="notes">
            <textarea
              id="notes"
              ref={notesInput}
              rows={4}
              placeholder={labels.order.notesPlaceholder}
              aria-invalid={errors.notes ? true : undefined}
              aria-describedby={describedBy('notes', 'notes-count')}
              value={values.notes}
              onChange={(event) => setField('notes', event.target.value)}
              onBlur={() => handleBlur('notes')}
              className={inputClass}
            />
            <p
              id="notes-count"
              aria-live={notesLength >= 450 ? 'polite' : 'off'}
              className={`text-right text-xs tabular-nums ${notesLength > MAX_NOTES ? 'text-red-600' : 'text-gray-500'}`}
            >
              {notesLength}/{MAX_NOTES}
            </p>
            <FieldError field="notes" error={errors.notes} />
          </Row>

          {current && (
            <p className="flex flex-wrap gap-x-4 gap-y-1 border-t border-gray-100 pt-3 text-xs text-gray-500">
              <span>{labels.order.created(formatTimestamp(current.createdAt))}</span>
              <span>{labels.order.updated(formatTimestamp(current.updatedAt))}</span>
            </p>
          )}
        </div>

        <div className="flex flex-col-reverse gap-2.5 sm:flex-row sm:justify-end">
          <button
            ref={cancelButton}
            type="button"
            onClick={handleCancel}
            className="rounded border border-gray-300 bg-white px-4 py-2 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
          >
            {labels.order.cancel}
          </button>
          <button
            type="submit"
            disabled={saving}
            className="rounded bg-gray-900 px-4 py-2 text-white focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none disabled:opacity-50"
          >
            {saving ? labels.order.saving : labels.order.save}
          </button>
        </div>
      </form>

      <DiscardChangesDialog
        open={confirmingDiscard}
        onDiscard={() => {
          const to = pendingTo.current ?? '/production-orders'
          pendingTo.current = null
          setConfirmingDiscard(false)
          if (to === pathname) {
            // Same route (e.g. "New production order" from an edited create form): nothing remounts, so reset here.
            setValues(initial)
            setErrors({})
          } else {
            navigate(to)
          }
        }}
        onKeepEditing={() => {
          const focusTarget = pendingTo.current ? returnFocus.current : cancelButton.current
          pendingTo.current = null
          setConfirmingDiscard(false)
          ;(focusTarget ?? cancelButton.current)?.focus()
        }}
      />
    </>
  )
}

function Row({
  label,
  htmlFor,
  labelId,
  required,
  children,
}: {
  label: string
  htmlFor?: string
  labelId?: string
  required?: boolean
  children: ReactNode
}) {
  const content = (
    <>
      {label}
      {required && (
        <span aria-hidden="true" className="text-red-700">
          {' '}
          *
        </span>
      )}
    </>
  )
  return (
    <div className="grid gap-1.5 sm:grid-cols-[9rem_1fr] sm:items-start sm:gap-3">
      {htmlFor ? (
        <label htmlFor={htmlFor} id={labelId} className="text-sm font-medium text-gray-700 sm:pt-2">
          {content}
        </label>
      ) : (
        <span id={labelId} className="text-sm font-medium text-gray-700">
          {content}
        </span>
      )}
      <div className="grid gap-1">{children}</div>
    </div>
  )
}

function FieldError({ field, error }: { field: Field; error: string | undefined }) {
  if (!error) {
    return null
  }
  return (
    <p id={`${field}-error`} className="text-sm text-red-600">
      {message(error)}
    </p>
  )
}

function LockHint({ id }: { id: string }) {
  return (
    <p id={id} className="text-xs text-gray-500">
      {labels.order.lockHint}
    </p>
  )
}
