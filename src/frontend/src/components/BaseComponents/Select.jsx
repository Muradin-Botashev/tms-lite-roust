import React, { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';

import { Dropdown, Form, Visibility } from 'semantic-ui-react';

import './style.scss';
import {
    clearFormLookup,
    getLookupRequest,
    listFromSelectSelector,
    listSelector,
    progressSelector,
    totalCounterSelector,
    valuesListSelector,
} from '../../ducks/lookup';
import { PAGE_SIZE } from '../../constants/settings';
import { debounce } from 'throttle-debounce';

const Select = ({
    value = {},
    onChange,
    placeholder = '',
    isDisabled,
    label,
    name,
    text,
    multiple,
    loading,
    clearable,
    source,
    isTranslate,
    error,
    textValue,
    noLabel,
    isRequired,
    autoComplete,
    children,
    upward,
    isBulkUpdateOnly,
    sourceParams,
}) => {
    const { t } = useTranslation();
    const dispatch = useDispatch();
    const context = useRef(null);
    let timer = useRef(null);

    let [open, setOpen] = useState(false);
    // let [items, setItems] = useState([]);
    let [counter, setCounter] = useState(PAGE_SIZE);
    let [searchQuery, setSearchQuery] = useState('');
    let [filter, setFilter] = useState('');

    const valuesList = useSelector(state =>
        listFromSelectSelector(state, source, t, filter, isTranslate, counter, isBulkUpdateOnly),
    );
    const totalCounter = useSelector(state =>
        totalCounterSelector(state, source, t, filter, isTranslate),
    );
    const progress = false;

    useEffect(() => {
        clearTimeout(timer.current);
        timer.current = setTimeout(() => {
            setFilter(searchQuery);
        }, 300);
    }, [searchQuery]);

    useEffect(() => {
        context.current.scrollTop = 0;
        setCounter(PAGE_SIZE);
    }, [filter]);

    const handleChange = (e, { value }) => {
        setSearchQuery('');
        toggle(false);
        onChange(e, { value: value ? value : null, name });
        handleClose();
    };

    const handleOpen = () => {
        dispatch(
            getLookupRequest({
                name: source,
                isForm: true,
                sourceParams,
            }),
        );
        toggle(true);
    };

    const handleClose = () => {
        context.current.scrollTop = 0;
        setCounter(PAGE_SIZE);
        // dispatch(clearFormLookup(source));
    };

    const handleBlur = () => {
        toggle(false);
        setSearchQuery('');
    };

    const toggle = value => {
        setOpen(value);
    };

    const scroll = () => {
        if (counter < totalCounter) {
            setCounter(prevState => prevState + PAGE_SIZE);
        }
    };

    const handleSearchChange = (e, { searchQuery }) => {
        setSearchQuery(searchQuery);
    };

    const handleFocus = () => {
        toggle(true);
        handleOpen();
    };

    return (
        <Form.Field>
            {!noLabel ? (
                <label className={isDisabled ? 'label-disabled' : null}>
                    <span
                        dangerouslySetInnerHTML={{
                            __html: `${t(text || name)}${isRequired ? ' *' : ''}`,
                        }}
                    />
                </label>
            ) : null}
            <div className={`form-select ${isDisabled ? 'form-select_disabled' : ''}`}>
                <Dropdown
                    placeholder={placeholder}
                    fluid
                    selection
                    loading={progress}
                    search
                    clearable={value && value.value}
                    value={value && value.value}
                    searchQuery={searchQuery}
                    text={value ? value.name : null}
                    error={error}
                    disabled={isDisabled}
                    closeOnChange={true}
                    closeOnBlur={true}
                    onBlur={handleBlur}
                    onClose={handleClose}
                    onOpen={handleOpen}
                    onSearchChange={handleSearchChange}
                    onFocus={handleFocus}
                    open={open}
                    upward={upward}
                    onChange={handleChange}
                >
                    <div role="listbox" className={`menu transition`} ref={context}>
                        {valuesList && valuesList.length ? (
                            valuesList.map(item => (
                                <Dropdown.Item
                                    style={item.isBulkUpdateOnly ? { color: '#9a0000' } : {}}
                                    key={item.value}
                                    selected={value && item.value === value.value}
                                    active={value && item.value === value.value}
                                    value={item.value}
                                    onClick={e => handleChange(e, { value: item })}
                                >
                                    {item.name}
                                </Dropdown.Item>
                            ))
                        ) : (
                            <div className="message">No results found.</div>
                        )}
                        <Visibility
                            continuous={false}
                            once={false}
                            context={context.current}
                            onTopVisible={scroll}
                            /*style={{
                                position: 'absolute',
                                bottom: 0,
                                left: 0,
                                right: 0,
                                zIndex: -1,
                            }}*/
                        />
                    </div>
                </Dropdown>
                {children && children}
            </div>
            {error && typeof error === 'string' && <span className="label-error">{error}</span>}
        </Form.Field>
    );
};

export default React.memo(Select);
